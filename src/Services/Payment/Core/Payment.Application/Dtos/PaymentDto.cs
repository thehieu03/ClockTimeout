using Payment.Domain.Enums;

namespace Payment.Application.Dtos;

public sealed class PaymentDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public string? TransactionId { get; set; }
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public PaymentMethod Method { get; set; }
    public string MethodName => Method.ToString();
    public string? ErrorMessage { get; set; }
    public string? RefundReason { get; set; }
    public string? RefundTransactionId { get; set; }
    public DateTimeOffset CreatedOnUtc { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset? LastModifiedOnUtc { get; set; }
    public string? LastModifiedBy { get; set; }
}
