using Payment.Domain.Enums;

namespace Payment.Application.Gateways.Models;

public record PaymentGatewayRequest
{
    public Guid PaymentId { get; init; }
    public Guid OrderId { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "VND";
    public PaymentMethod Method { get; init; }
    public string? Description { get; init; }
    public string? ReturnUrl { get; init; }
    public string? CancelUrl { get; init; }
    public Dictionary<string, string> Metadata { get; init; } = new();
}
