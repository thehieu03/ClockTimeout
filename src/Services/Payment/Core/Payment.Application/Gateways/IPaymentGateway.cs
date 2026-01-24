using Payment.Application.Gateways.Models;
using Payment.Domain.Enums;

namespace Payment.Application.Gateways;

public interface IPaymentGateway
{
    
    PaymentMethod SupportedMethod { get; }

    Task<PaymentGatewayResult> ProcessPaymentAsync(
        PaymentGatewayRequest request,
        CancellationToken cancellationToken = default);

    Task<PaymentGatewayResult> VerifyPaymentAsync(
        string transactionId,
        CancellationToken cancellationToken = default);

    Task<PaymentGatewayResult> RefundPaymentAsync(
        string transactionId,
        decimal amount,
        CancellationToken cancellationToken = default);
}
