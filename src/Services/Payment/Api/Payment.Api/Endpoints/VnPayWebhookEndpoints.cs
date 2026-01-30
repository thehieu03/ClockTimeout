using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Payment.Application.Features.Payment.Commands;
using Payment.Infrastructure.Configurations;
using Payment.Infrastructure.Gateways.VnPay;
using Payment.Infrastructure.Gateways.VnPay.Models;
using Common.ValueObjects;
using System.Text.Json;

namespace Payment.Api.Endpoints;

/// <summary>
/// VNPay Payment Gateway Webhook Endpoints
/// </summary>
public class VnPayWebhookEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/payments/vnpay")
            .WithTags("VNPay Webhooks")
            .WithOpenApi();

        // IPN Endpoint (GET in VNPay's case)
        group.MapGet("/ipn", HandleIpnAsync)
            .AllowAnonymous()
            .WithName("VnPayIPN")
            .WithSummary("Handle VNPay IPN")
            .WithDescription("Called by VNPay to notify payment result");

        // Return URL
        group.MapGet("/return", HandleReturnUrlAsync)
            .AllowAnonymous()
            .WithName("VnPayReturn")
            .WithSummary("Handle VNPay Return URL");
    }

    private async Task<IResult> HandleIpnAsync(
        [AsParameters] VnPayIpnRequest request,
        ISender sender,
        Payment.Infrastructure.Data.ApplicationDbContext dbContext,
        IOptions<VnPaySettings> vnpaySettings,
        ILogger<VnPayWebhookEndpoints> logger)
    {
        var requestId = request.vnp_TxnRef; // Use TxnRef as unique request ID for VNPay
        const string gateway = "VnPay";
        var payload = JsonSerializer.Serialize(request);

        logger.LogInformation(
            "Received VNPay IPN: TxnRef={TxnRef}, ResponseCode={ResponseCode}, Amount={Amount}",
            request.vnp_TxnRef, request.vnp_ResponseCode, request.vnp_Amount);

        // Step 1: Audit Log & Idempotency Check
        var existingLog = await dbContext.WebhookLogs
            .FirstOrDefaultAsync(x => x.Gateway == gateway && x.RequestId == requestId);

        if (existingLog == null)
        {
            existingLog = Payment.Domain.Entities.PaymentWebhookLog.Create(gateway, requestId, payload);
            dbContext.WebhookLogs.Add(existingLog);
            await dbContext.SaveChangesAsync();
            logger.LogInformation("Created webhook log for TxnRef={TxnRef}", requestId);
        }
        else if (existingLog.IsProcessed)
        {
            logger.LogInformation("IPN {TxnRef} already processed. Returning success.", requestId);
            return Results.Json(new { RspCode = "00", Message = "Already processed" });
        }

        try
        {
            // Step 2: Verify Signature
            var isValidSignature = VerifyVnPaySignature(request, vnpaySettings.Value, logger);

            if (!isValidSignature)
            {
                logger.LogError(
                    "Invalid VNPay signature. TxnRef={TxnRef}. Potential fraud!",
                    request.vnp_TxnRef);

                existingLog.MarkFailed("Invalid Signature");
                await dbContext.SaveChangesAsync();

                return Results.Json(new
                {
                    RspCode = "97",
                    Message = "Invalid Signature"
                });
            }

            // Step 3: Parse PaymentId
            if (!Guid.TryParse(request.vnp_TxnRef, out var paymentId))
            {
                logger.LogError("Invalid TxnRef format: {TxnRef}", request.vnp_TxnRef);

                existingLog.MarkFailed("Invalid TxnRef format");
                await dbContext.SaveChangesAsync();

                return Results.Json(new
                {
                    RspCode = "99",
                    Message = "Invalid TxnRef"
                });
            }

            // Step 4: Check success
            var isSuccess = request.vnp_ResponseCode == "00" &&
                          request.vnp_TransactionStatus == "00";

            // Step 5: Send command
            var command = new HandlePaymentCallbackCommand(
                PaymentId: paymentId,
                IsSuccess: isSuccess,
                TransactionId: request.vnp_TransactionNo,
                ResultCode: request.vnp_ResponseCode,
                ResultMessage: GetVnPayMessage(request.vnp_ResponseCode),
                RawResponse: payload,
                Gateway: gateway,
                Actor: Actor.System("Payment")
            );

            var result = await sender.Send(command);

            // Step 6: Mark log and return
            if (result.Success)
            {
                existingLog.MarkProcessed();
                await dbContext.SaveChangesAsync();

                logger.LogInformation("VNPay IPN processed successfully");
                return Results.Json(new
                {
                    RspCode = "00",
                    Message = "Confirm Success"
                });
            }
            else
            {
                existingLog.MarkFailed(result.Message);
                await dbContext.SaveChangesAsync();

                logger.LogWarning("VNPay IPN processing failed: {Reason}", result.Message);
                return Results.Json(new
                {
                    RspCode = "99",
                    Message = result.Message
                });
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing VNPay IPN");

            existingLog.MarkFailed(ex.Message);
            await dbContext.SaveChangesAsync();

            return Results.Json(new
            {
                RspCode = "99",
                Message = "Internal Error"
            });
        }
    }

    private async Task<IResult> HandleReturnUrlAsync(
        [AsParameters] VnPayIpnRequest query,
        IWebHostEnvironment env,
        ILogger<VnPayWebhookEndpoints> logger)
    {
        logger.LogInformation("VNPay ReturnUrl: TxnRef={TxnRef}, ResponseCode={Code}",
            query.vnp_TxnRef, query.vnp_ResponseCode);

        var isSuccess = query.vnp_ResponseCode == "00";
        var message = GetVnPayMessage(query.vnp_ResponseCode);
        var statusClass = isSuccess ? "success" : "failed";
        var statusText = isSuccess ? "Thành công" : "Thất bại";
        var amount = query.vnp_Amount / 100; // VNPay amount is multiplied by 100

        // Load HTML template from file
        var templatePath = Path.Combine(env.ContentRootPath, "Templates", "vnpay_result.html");
        var html = await File.ReadAllTextAsync(templatePath);

        // Replace placeholders
        html = html
            .Replace("{{STATUS_CLASS}}", statusClass)
            .Replace("{{STATUS_TEXT}}", statusText)
            .Replace("{{TXN_REF}}", query.vnp_TxnRef)
            .Replace("{{AMOUNT}}", amount.ToString("N0"))
            .Replace("{{TRANS_NO}}", query.vnp_TransactionNo)
            .Replace("{{MESSAGE}}", message);

        return Results.Content(html, "text/html");
    }

    private bool VerifyVnPaySignature(
        VnPayIpnRequest request,
        VnPaySettings settings,
        ILogger<VnPayWebhookEndpoints> logger)
    {
        try
        {
            // Build query string from all vnp_* parameters except vnp_SecureHash
            var vnpParams = new Dictionary<string, string>
            {
                { "vnp_TxnRef", request.vnp_TxnRef },
                { "vnp_Amount", request.vnp_Amount.ToString() },
                { "vnp_OrderInfo", request.vnp_OrderInfo },
                { "vnp_ResponseCode", request.vnp_ResponseCode },
                { "vnp_TransactionNo", request.vnp_TransactionNo },
                { "vnp_BankCode", request.vnp_BankCode },
                { "vnp_PayDate", request.vnp_PayDate },
                { "vnp_TmnCode", request.vnp_TmnCode },
                { "vnp_TransactionStatus", request.vnp_TransactionStatus }
            };

            // Sort by key
            var sortedParams = vnpParams.OrderBy(x => x.Key);

            // Build query string
            var queryString = string.Join("&",
                sortedParams.Select(x => $"{x.Key}={x.Value}"));

            logger.LogDebug("VNPay signature raw data: {Data}", queryString);

            // Compute HMAC SHA512
            var expectedHash = VnPayHelper.ComputeHmacSha512(
                queryString,
                settings.HashSecret);

            logger.LogDebug("Expected: {Expected}, Received: {Received}",
                expectedHash, request.vnp_SecureHash);

            return expectedHash.Equals(request.vnp_SecureHash,
                StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error verifying VNPay signature");
            return false;
        }
    }

    private string GetVnPayMessage(string responseCode)
    {
        return responseCode switch
        {
            "00" => "Giao dịch thành công",
            "07" => "Trừ tiền thành công. Giao dịch bị nghi ngờ (liên quan tới lừa đảo, giao dịch bất thường).",
            "09" => "Giao dịch không thành công do: Thẻ/Tài khoản của khách hàng chưa đăng ký dịch vụ InternetBanking tại ngân hàng.",
            "10" => "Giao dịch không thành công do: Khách hàng xác thực thông tin thẻ/tài khoản không đúng quá 3 lần",
            "11" => "Giao dịch không thành công do: Đã hết hạn chờ thanh toán.",
            "12" => "Giao dịch không thành công do: Thẻ/Tài khoản của khách hàng bị khóa.",
            "13" => "Giao dịch không thành công do Quý khách nhập sai mật khẩu xác thực giao dịch (OTP).",
            "24" => "Giao dịch không thành công do: Khách hàng hủy giao dịch",
            "51" => "Giao dịch không thành công do: Tài khoản của quý khách không đủ số dư để thực hiện giao dịch.",
            "65" => "Giao dịch không thành công do: Tài khoản của Quý khách đã vượt quá hạn mức giao dịch trong ngày.",
            "75" => "Ngân hàng thanh toán đang bảo trì.",
            "79" => "Giao dịch không thành công do: KH nhập sai mật khẩu thanh toán quá số lần quy định.",
            _ => "Lỗi không xác định"
        };
    }
}
