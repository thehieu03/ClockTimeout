using Payment.Domain.Enums;

namespace Payment.Application.Dtos;

public sealed class CreatePaymentDto
{
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod Method { get; set; }
}
