using Microsoft.Extensions.Logging;
using Payment.Application.Gateways;
using Payment.Application.Gateways.Models;
using Payment.Domain.Enums;

namespace Payment.Infrastructure.Gateways;

/// <summary>
/// Mock payment gateway for testing purposes.
/// Simulates payment processing with configurable success rate.
/// </summary>
public class MockPaymentGateway : IPaymentGateway
{
    private readonly ILogger<MockPaymentGateway> _logger;
    private readonly Random _random = new();

    // Configure success rate (0.0 to 1.0)
    private const double SuccessRate = 0.9;

    public MockPaymentGateway(ILogger<MockPaymentGateway> logger)
    {
        _logger = logger;
    }

    public PaymentMethod SupportedMethod => PaymentMethod.VnPay;

    public async Task<PaymentGatewayResult> ProcessPaymentAsync(
        PaymentGatewayRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "[MockGateway] Processing payment {PaymentId} for amount {Amount}",
            request.PaymentId,
            request.Amount);

        // Simulate network delay
        await Task.Delay(TimeSpan.FromMilliseconds(500), cancellationToken);

        // Simulate random success/failure based on success rate
        var isSuccess = _random.NextDouble() < SuccessRate;

        if (isSuccess)
        {
            var transactionId = $"MOCK_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid():N}"[..32];

            _logger.LogInformation(
                "[MockGateway] Payment {PaymentId} processed successfully. TransactionId: {TransactionId}",
                request.PaymentId,
                transactionId);

            return PaymentGatewayResult.Success(
                transactionId: transactionId,
                redirectUrl: request.ReturnUrl,
                rawResponse: $"{{\"status\":\"success\",\"transaction_id\":\"{transactionId}\"}}"
            );
        }
        else
        {
            var errorCode = GetRandomErrorCode();
            var errorMessage = GetErrorMessage(errorCode);

            _logger.LogWarning(
                "[MockGateway] Payment {PaymentId} failed. Error: {ErrorCode}",
                request.PaymentId,
                errorCode);

            return PaymentGatewayResult.Failure(
                errorCode: errorCode,
                errorMessage: errorMessage,
                rawResponse: $"{{\"status\":\"failed\",\"error_code\":\"{errorCode}\",\"message\":\"{errorMessage}\"}}"
            );
        }
    }

    public async Task<PaymentGatewayResult> VerifyPaymentAsync(
        string transactionId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[MockGateway] Verifying transaction {TransactionId}", transactionId);

        await Task.Delay(TimeSpan.FromMilliseconds(200), cancellationToken);

        // Always return success for mock
        return PaymentGatewayResult.Success(transactionId);
    }

    public async Task<PaymentGatewayResult> RefundPaymentAsync(
        string transactionId,
        decimal amount,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "[MockGateway] Refunding {Amount} for transaction {TransactionId}",
            amount,
            transactionId);

        await Task.Delay(TimeSpan.FromMilliseconds(300), cancellationToken);

        var refundId = $"REFUND_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid():N}"[..32];
        return PaymentGatewayResult.Success(refundId);
    }

    private string GetRandomErrorCode()
    {
        var errorCodes = new[] { "INSUFFICIENT_FUNDS", "CARD_DECLINED", "EXPIRED_CARD", "NETWORK_ERROR" };
        return errorCodes[_random.Next(errorCodes.Length)];
    }

    private static string GetErrorMessage(string errorCode) => errorCode switch
    {
        "INSUFFICIENT_FUNDS" => "Insufficient funds in account",
        "CARD_DECLINED" => "Card was declined by issuer",
        "EXPIRED_CARD" => "Card has expired",
        "NETWORK_ERROR" => "Network error occurred during processing",
        _ => "Unknown error occurred"
    };
}
