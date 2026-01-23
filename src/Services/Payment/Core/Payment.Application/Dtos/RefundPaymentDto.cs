namespace Payment.Application.Dtos;

public sealed class RefundPaymentDto
{
    public string? RefundReason { get; set; }
    public string? RefundTransactionId { get; set; }
}
