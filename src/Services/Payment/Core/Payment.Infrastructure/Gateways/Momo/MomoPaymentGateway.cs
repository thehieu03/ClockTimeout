using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Payment.Application.Gateways;
using Payment.Application.Gateways.Models;
using Payment.Domain.Enums;
using Payment.Infrastructure.Configurations;

namespace Payment.Infrastructure.Gateways.Momo;

public class MomoPaymentGateway(MomoSettings settings, ILogger<MomoPaymentGateway> logger, HttpClient httpclient) : IPaymentGateway
{
    public PaymentMethod SupportedMethod => PaymentMethod.Momo;

    public async Task<PaymentGatewayResult> ProcessPaymentAsync(PaymentGatewayRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation(
                "[Momo] Creating payment for PaymentId: {PaymentId}, Amount: {Amount}",
                request.PaymentId,
                request.Amount);

            // Generate unique IDs
            var requestId = MomoHelper.GenerateRequestId();
            var orderId = $"MOMO_{request.PaymentId:N}".Substring(0, 50);

            // Prepare extra data
            var extraData = MomoHelper.EncodeExtraData(new Dictionary<string, string>
            {
                { "paymentId", request.PaymentId.ToString() },
                { "orderId", request.OrderId.ToString() }
            });

            // Determine URLs
            var returnUrl = !string.IsNullOrEmpty(request.ReturnUrl)
                ? request.ReturnUrl
                : settings.ReturnUrl;

            var notifyUrl = settings.NotifyUrl;

            // Build signature raw data
            var rawSignature = MomoHelper.BuildCreatePaymentRawData(
                accessKey: settings.AccessKey,
                amount: request.Amount,
                extraData: extraData,
                ipnUrl: notifyUrl,
                orderId: orderId,
                orderInfor: request.Description ?? $"Thanh toán đơn hàng {request.OrderId}",
                partnerCode: settings.PartnerCode,
                redirectUrl: returnUrl,
                requestId: requestId,
                requestType: settings.RequestType
            );

            // Generate signature
            var signature = MomoHelper.GenerateSignature(rawSignature, settings.SecretKey);

            logger.LogDebug("[Momo] Raw signature data: {RawData}", rawSignature);

            // Build request
            var momoRequest = new MomoCreatePaymentRequest
            {
                PartnerCode = settings.PartnerCode,
                PartnerName = settings.PartnerName,
                StoreId = settings.StoreId,
                RequestId = requestId,
                Amount = (long)request.Amount,
                OrderId = orderId,
                OrderInfo = request.Description ?? $"Thanh toán đơn hàng {request.OrderId}",
                RedirectUrl = returnUrl,
                IpnUrl = notifyUrl,
                RequestType = settings.RequestType,
                ExtraData = extraData,
                Lang = "vi",
                Signature = signature,
                OrderExpireTime = settings.ExpiryInMinutes
            };

            // Add user info if available
            if (request.Metadata.TryGetValue("CustomerName", out var name) ||
                request.Metadata.TryGetValue("CustomerPhone", out var phone) ||
                request.Metadata.TryGetValue("CustomerEmail", out var email))
            {
                momoRequest.UserInfo = new MomoUserInfo
                {
                    Name = request.Metadata.GetValueOrDefault("CustomerName"),
                    PhoneNumber = request.Metadata.GetValueOrDefault("CustomerPhone"),
                    Email = request.Metadata.GetValueOrDefault("CustomerEmail")
                };
            }

            // Call Momo API
            var apiUrl = $"{settings.ApiEndpoint}/create";

            logger.LogInformation("[Momo] Calling API: {Url}", apiUrl);

            var response = await httpclient.PostAsJsonAsync(apiUrl, momoRequest, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            logger.LogDebug("[Momo] API Response: {Response}", responseContent);

            var momoResponse = JsonSerializer.Deserialize<MomoCreatePaymentResponse>(responseContent);

            if (momoResponse == null)
            {
                logger.LogError("[Momo] Failed to parse response");
                return PaymentGatewayResult.Failure("PARSE_ERROR", "Failed to parse Momo response", responseContent);
            }

            if (momoResponse.IsSuccess)
            {
                logger.LogInformation(
                    "[Momo] Payment created successfully. OrderId: {OrderId}, PayUrl: {PayUrl}",
                    orderId,
                    momoResponse.PayUrl);

                return PaymentGatewayResult.Success(
                    transactionId: orderId,
                    redirectUrl: momoResponse.PayUrl,
                    rawResponse: responseContent
                );
            }
            else
            {
                var errorMessage = MomoHelper.GetResultMessage(momoResponse.ResultCode);

                logger.LogWarning(
                    "[Momo] Payment creation failed. ResultCode: {ResultCode}, Message: {Message}",
                    momoResponse.ResultCode,
                    errorMessage);

                return PaymentGatewayResult.Failure(
                    errorCode: momoResponse.ResultCode.ToString(),
                    errorMessage: errorMessage,
                    rawResponse: responseContent
                );
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[Momo] Error creating payment for PaymentId: {PaymentId}", request.PaymentId);

            return PaymentGatewayResult.Failure(
                errorCode: "MOMO_ERROR",
                errorMessage: ex.Message
            );
        }
    }

    public Task<PaymentGatewayResult> RefundPaymentAsync(string transactionId, decimal amount, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<PaymentGatewayResult> VerifyPaymentAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(PaymentGatewayResult.Failure("NOT_IMPLEMENTED", "Verify not needed for redirect flow yet"));
    }
}
