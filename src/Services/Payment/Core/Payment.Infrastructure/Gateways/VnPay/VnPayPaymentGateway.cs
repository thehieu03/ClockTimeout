using Microsoft.Extensions.Logging;
using System.Text.Json;
using Payment.Application.Gateways;
using Payment.Application.Gateways.Models;
using Payment.Domain.Enums;
using Payment.Infrastructure.Configurations;

namespace Payment.Infrastructure.Gateways.VnPay;

public class VnPayPaymentGateway(VnPaySettings settings, ILogger<VnPayPaymentGateway> logger) : IPaymentGateway
{

    public PaymentMethod SupportedMethod { get; } = PaymentMethod.VnPay;
    public Task<PaymentGatewayResult> ProcessPaymentAsync(PaymentGatewayRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("[VNPay] Creating payment URL for PaymentId: {PaymentId}, Amount {Amount}"
                , request.PaymentId
                , request.Amount);
            // Generate unique transection reference
            var txnRef = $"{request.PaymentId:N}".Substring(0, 20);
            // Get client IP (should be passed from request in production)
            var ipAddress = request.Metadata.GetValueOrDefault("IpAddress", "127.0.0.1");
            // Determine return URL
            var returnUrl = !string.IsNullOrEmpty(request.ReturnUrl)
                ? request.ReturnUrl
                : $"{settings.ReturnUrl}?paymentId={request.PaymentId}";
            // Build payment URL
            var paymentUrl = VnPayHelper.BuildPaymentUrl(
                baseUrl: settings.PaymentUrl,
                tmnCode: settings.TmnCode,
                hashSecret: settings.HashSecret,
                txnRef: txnRef,
                amount: request.Amount,
                orderInfo: request.Description ?? $"Thanh toan don hang {request.OrderId}",
                returnUrl: returnUrl,
                ipAddress: ipAddress,
                locale: settings.Locale,
                currencyCode: settings.CurrencyCode,
                version: settings.Version,
                bankCode: request.Metadata.GetValueOrDefault("BankCode")
            );

            logger.LogInformation(
                "[VNPay] Payment URL created for PaymentId: {PaymentId}, TxnRef: {TxnRef}",
                request.PaymentId,
                txnRef);

            // Return redirect URL - payment will be completed via callback
            return Task.FromResult(PaymentGatewayResult.Success(
                transactionId: txnRef,
                redirectUrl: paymentUrl,
                rawResponse: JsonSerializer.Serialize(new { txnRef, paymentUrl })
            ));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[VNPay] Error creating payment URL for PaymentId: {PaymentId}", request.PaymentId);

            return Task.FromResult(PaymentGatewayResult.Failure(
                errorCode: "VNPAY_ERROR",
                errorMessage: ex.Message
            ));
        }
    }
    public Task<PaymentGatewayResult> VerifyPaymentAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("[VNPay] Verifying payment for TxnRef: {TxnRef}", transactionId);
            var queryParams = new Dictionary<string, string>
            {
                { "vnp_RequestId", Guid.NewGuid().ToString("N") },
                { "vnp_Version", settings.Version },
                { "vnp_Command", "querydr" },
                { "vnp_TmnCode", settings.TmnCode },
                { "vnp_TxnRef", transactionId },
                { "vnp_OrderInfo", $"Query transaction {transactionId}" },
                { "vnp_TransactionDate", DateTime.Now.ToString("yyyyMMddHHmmss") },
                { "vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss") },
                { "vnp_IpAddr", "127.0.0.1" }
            };
            logger.LogWarning("[VNPay] Query API not fully implemented. Simulating success for TxnRef: {TxnRef}", transactionId);
            return Task.FromResult(PaymentGatewayResult.Success(transactionId));

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[VNPay] Error verifying payment for TxnRef: {TxnRef}", transactionId);

            return Task.FromResult(PaymentGatewayResult.Failure(
                errorCode: "VNPAY_VERIFY_ERROR",
                errorMessage: ex.Message
            ));
        }
    }
    public Task<PaymentGatewayResult> RefundPaymentAsync(string transactionId, decimal amount, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation(
                "[VNPay] Refunding transaction: {TransactionId}, Amount: {Amount}",
                transactionId,
                amount);

            // VNPay refund requires calling their refund API
            // This is a simplified placeholder

            var refundTxnRef = $"RF{transactionId}";

            logger.LogWarning("[VNPay] Refund API implementation is simplified. Manual refund may be required.");

            return Task.FromResult(PaymentGatewayResult.Success(refundTxnRef));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[VNPay] Error refunding transaction: {TransactionId}", transactionId);
            return Task.FromResult(PaymentGatewayResult.Failure("REFUND_ERROR", ex.Message));
        }
    }
}