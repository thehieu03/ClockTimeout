using Microsoft.Extensions.Logging;
using Payment.Application.Gateways;
using Payment.Application.Gateways.Models;
using Payment.Domain.Enums;

namespace Payment.Infrastructure.Gateways;

/// <summary>
/// Cash On Delivery gateway - marks payment as pending until delivery.
/// </summary>
public class CodPaymentGateway : IPaymentGateway
{
    private readonly ILogger<CodPaymentGateway> _logger;

    public CodPaymentGateway(ILogger<CodPaymentGateway> logger)
    {
        _logger = logger;
    }

    public PaymentMethod SupportedMethod => PaymentMethod.Cod;

    public Task<PaymentGatewayResult> ProcessPaymentAsync(
        PaymentGatewayRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "[COD] Payment {PaymentId} created for COD. Amount: {Amount}",
            request.PaymentId,
            request.Amount);

        // COD payments are always "successful" - they complete on delivery
        var transactionId = $"COD_{request.OrderId:N}"[..32];

        return Task.FromResult(PaymentGatewayResult.Success(
            transactionId: transactionId,
            rawResponse: "{\"status\":\"cod_pending\",\"message\":\"Payment will be collected on delivery\"}"
        ));
    }

    public Task<PaymentGatewayResult> VerifyPaymentAsync(
        string transactionId,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(PaymentGatewayResult.Success(transactionId));
    }

    public Task<PaymentGatewayResult> RefundPaymentAsync(
        string transactionId,
        decimal amount,
        CancellationToken cancellationToken = default)
    {
        // COD refunds are handled manually
        return Task.FromResult(PaymentGatewayResult.Success($"REFUND_{transactionId}"));
    }
}
