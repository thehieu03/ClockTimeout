using Payment.Domain.Enums;

namespace Payment.Application.Gateways;

public interface IPaymentGatewayFactory
{
    IPaymentGateway GetGateway(PaymentMethod method);
    bool IsSupported(PaymentMethod method);
}
