using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Payment.Application.Features.Payment.Commands;
using Payment.Infrastructure.Configurations;
using Payment.Infrastructure.Gateways.Momo;
using Payment.Infrastructure.Gateways.Momo.Models;
using Common.ValueObjects;
using System.Text.Json;

namespace Payment.Api.Endpoints;

public class MomoWebhookEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/payments/momo")
            .WithTags("Momo Webhooks")
            .WithOpenApi();

        group.MapPost("/ipn", HandleIpnAsync)
            .AllowAnonymous()
            .WithName("MomoIPN")
            .WithSummary("Handle Momo IPN (Instant Payment Notification)")
            .WithDescription("This endpoint is called by Momo server to notify payment result. Must verify signature.");

        group.MapGet("/return", HandleReturnUrlAsync)
            .AllowAnonymous()
            .WithName("MomoReturn")
            .WithSummary("Handle Momo Return URL")
            .WithDescription("User is redirected here after payment. Use for UI feedback only.");
    }

    private async Task<IResult> HandleIpnAsync(
        [FromBody] MomoIpnRequest request,
        ISender sender,
        Payment.Infrastructure.Data.ApplicationDbContext dbContext,
        IOptions<MomoSettings> momoSettings,
        ILogger<MomoWebhookEndpoints> logger)
    {
        var requestId = request.RequestId;
        var orderId = request.OrderId;
        const string gateway = "Momo";
        var payload = JsonSerializer.Serialize(request);

        logger.LogInformation(
            "Received Momo IPN: OrderId={OrderId}, RequestId={RequestId}, ResultCode={ResultCode}, Amount={Amount}",
            orderId, requestId, request.ResultCode, request.Amount);

        // Step 1: Audit Log (Always save first) & Idempotency Check
        var existingLog = await dbContext.WebhookLogs
            .FirstOrDefaultAsync(x => x.Gateway == gateway && x.RequestId == requestId);

        if (existingLog == null)
        {
            existingLog = Payment.Domain.Entities.PaymentWebhookLog.Create(gateway, requestId, payload);
            dbContext.WebhookLogs.Add(existingLog);
            await dbContext.SaveChangesAsync();
            logger.LogInformation("Created webhook log for RequestId={RequestId}", requestId);
        }
        else if (existingLog.IsProcessed)
        {
            logger.LogInformation("IPN {RequestId} already processed. Returning 204.", requestId);
            return Results.NoContent();
        }

        try
        {
            // Step 2: Verify Signature (CRITICAL FOR SECURITY)
            var isValidSignature = VerifyMomoIpnSignature(request, momoSettings.Value, logger);

            if (!isValidSignature)
            {
                logger.LogError(
                    "Invalid Momo IPN signature. OrderId={OrderId}, RequestId={RequestId}. Potential fraud attempt!",
                    orderId, requestId);

                existingLog.MarkFailed("Invalid signature");
                await dbContext.SaveChangesAsync();

                return Results.Json(
                    new { message = "Invalid signature" },
                    statusCode: 400);
            }

            logger.LogInformation("Momo IPN signature verified successfully");

            // Step 3: Parse PaymentId from OrderId
            if (!Guid.TryParse(orderId, out var paymentId))
            {
                logger.LogError(
                    "Invalid OrderId format in Momo IPN: {OrderId}",
                    orderId);

                existingLog.MarkFailed("Invalid OrderId format");
                await dbContext.SaveChangesAsync();

                return Results.Json(
                    new { message = "Invalid OrderId format" },
                    statusCode: 400);
            }

            var isSuccess = request.ResultCode == 0;
            var command = new HandlePaymentCallbackCommand(
                PaymentId: paymentId,
                IsSuccess: isSuccess,
                TransactionId: request.TransId.ToString(),
                ResultCode: request.ResultCode.ToString(),
                ResultMessage: request.Message,
                RawResponse: payload,
                Gateway: gateway,
                Actor: Actor.System("Payment")
            );

            var result = await sender.Send(command);

            // Step 4: Mark log as processed
            if (result.Success)
            {
                existingLog.MarkProcessed();
                await dbContext.SaveChangesAsync();

                logger.LogInformation(
                    "Momo IPN processed successfully. OrderId={OrderId}, WasAlreadyProcessed={AlreadyProcessed}",
                    orderId, result.WasAlreadyProcessed);

                return Results.NoContent();
            }
            else
            {
                existingLog.MarkFailed(result.Message);
                await dbContext.SaveChangesAsync();

                logger.LogWarning(
                    "Momo IPN processing failed. OrderId={OrderId}, Reason={Reason}",
                    orderId, result.Message);

                return Results.Json(
                    new { message = result.Message },
                    statusCode: 500);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Unexpected error processing Momo IPN. OrderId={OrderId}, RequestId={RequestId}",
                orderId, requestId);

            existingLog.MarkFailed(ex.Message);
            await dbContext.SaveChangesAsync();

            return Results.Json(
                new { message = "Internal server error" },
                statusCode: 500);
        }
    }
    private async Task<IResult> HandleReturnUrlAsync(
        [AsParameters] MomoReturnUrlQuery query,
        IWebHostEnvironment env,
        ILogger<MomoWebhookEndpoints> logger)
    {
        logger.LogInformation(
            "Received Momo ReturnUrl: OrderId={OrderId}, ResultCode={ResultCode}",
            query.OrderId, query.ResultCode);

        var isSuccess = query.ResultCode == 0;
        var statusClass = isSuccess ? "success" : "failed";
        var statusText = isSuccess ? "Thành công" : "Thất bại";

        // Load HTML template from file
        var templatePath = Path.Combine(env.ContentRootPath, "Templates", "momo_result.html");
        var html = await File.ReadAllTextAsync(templatePath);

        // Replace placeholders
        html = html
            .Replace("{{STATUS_CLASS}}", statusClass)
            .Replace("{{STATUS_TEXT}}", statusText)
            .Replace("{{ORDER_ID}}", query.OrderId)
            .Replace("{{AMOUNT}}", query.Amount.ToString("N0"))
            .Replace("{{TRANS_ID}}", query.TransId.ToString())
            .Replace("{{MESSAGE}}", query.Message);

        return Results.Content(html, "text/html");
    }
    private bool VerifyMomoIpnSignature(
        MomoIpnRequest request,
        MomoSettings settings,
        ILogger<MomoWebhookEndpoints> logger)
    {
        try
        {
            var rawSignature = $"accessKey={settings.AccessKey}" +
                             $"&amount={request.Amount}" +
                             $"&extraData={request.ExtraData}" +
                             $"&message={request.Message}" +
                             $"&orderId={request.OrderId}" +
                             $"&orderInfo={request.OrderInfo}" +
                             $"&orderType={request.OrderType}" +
                             $"&partnerCode={request.PartnerCode}" +
                             $"&payType={request.PayType}" +
                             $"&requestId={request.RequestId}" +
                             $"&responseTime={request.ResponseTime}" +
                             $"&resultCode={request.ResultCode}" +
                             $"&transId={request.TransId}";

            logger.LogDebug("Momo IPN raw signature string: {RawSignature}", rawSignature);

            var expectedSignature = MomoHelper.ComputeHmacSha256(rawSignature, settings.SecretKey);

            logger.LogDebug(
                "Signature comparison - Expected: {Expected}, Received: {Received}",
                expectedSignature, request.Signature);
            return expectedSignature.Equals(request.Signature, StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error verifying Momo IPN signature");
            return false;
        }
    }
}
