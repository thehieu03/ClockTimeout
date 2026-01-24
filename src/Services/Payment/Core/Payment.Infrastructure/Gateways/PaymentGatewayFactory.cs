using Payment.Application.Gateways;
using Payment.Domain.Enums;

namespace Payment.Infrastructure.Gateways;

public class PaymentGatewayFactory : IPaymentGatewayFactory
{
    private readonly IEnumerable<IPaymentGateway> _gateways;

    public PaymentGatewayFactory(IEnumerable<IPaymentGateway> gateways)
    {
        _gateways = gateways;
    }

    public IPaymentGateway GetGateway(PaymentMethod method)
    {
        var gateway = _gateways.FirstOrDefault(g => g.SupportedMethod == method);

        if (gateway is null)
        {
            throw new NotSupportedException($"Payment method {method} is not supported");
        }

        return gateway;
    }

    public bool IsSupported(PaymentMethod method)
    {
        return _gateways.Any(g => g.SupportedMethod == method);
    }
}
