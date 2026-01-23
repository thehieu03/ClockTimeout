using BuildingBlocks.Abstractions;
using Payment.Domain.Enums;
using Payment.Domain.Events;

namespace Payment.Domain.Entities;

public sealed class PaymentEntity : Aggregate<Guid>
{
    public Guid OrderId { get; private set; }
    public string? TransactionId { get; private set; }
    public decimal Amount { get; private set; }
    public PaymentStatus Status { get; private set; }
    public PaymentMethod Method { get; private set; }
    public string? ErrorMessage { get; private set; }
    public string? RefundReason { get; private set; }
    public string? RefundTransactionId { get; private set; }

    private PaymentEntity() { }

    public static PaymentEntity Create(Guid orderId, decimal amount, PaymentMethod method, string? createdBy = null)
    {
        var payment = new PaymentEntity
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            Amount = amount,
            Method = method,
            Status = PaymentStatus.Pending,
            CreatedOnUtc = DateTimeOffset.UtcNow,
            CreatedBy = createdBy
        };

        payment.AddDomainEvent(new PaymentCreatedDomainEvent(payment.Id, orderId, amount));
        return payment;
    }

    public void Complete(string transactionId, string? modifiedBy = null)
    {
        if (Status != PaymentStatus.Pending)
            throw new InvalidOperationException("Only pending payments can be completed.");

        TransactionId = transactionId;
        Status = PaymentStatus.Completed;
        LastModifiedOnUtc = DateTimeOffset.UtcNow;
        LastModifiedBy = modifiedBy;

        AddDomainEvent(new PaymentCompletedDomainEvent(Id, OrderId, transactionId));
    }

    public void MarkAsFailed(string errorMessage, string? modifiedBy = null)
    {
        if (Status != PaymentStatus.Pending)
            throw new InvalidOperationException("Only pending payments can be marked as failed.");

        Status = PaymentStatus.Failed;
        ErrorMessage = errorMessage;
        LastModifiedOnUtc = DateTimeOffset.UtcNow;
        LastModifiedBy = modifiedBy;

        AddDomainEvent(new PaymentFailedDomainEvent(Id, OrderId, errorMessage));
    }

    public void Refund(string? refundReason, string? refundTransactionId = null, string? modifiedBy = null)
    {
        if (Status != PaymentStatus.Completed)
            throw new InvalidOperationException("Only completed payments can be refunded.");

        Status = PaymentStatus.Refunded;
        RefundReason = refundReason;
        RefundTransactionId = refundTransactionId;
        LastModifiedOnUtc = DateTimeOffset.UtcNow;
        LastModifiedBy = modifiedBy;

        AddDomainEvent(new PaymentRefundedDomainEvent(Id, OrderId, refundReason));
    }

    private void AddDomainEvent(IDomainEvent domainEvent)
    {
        // Access the protected list from Aggregate base class
        var domainEventsField = typeof(Aggregate<Guid>)
            .GetField("_domainEvents", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var domainEvents = domainEventsField?.GetValue(this) as List<IDomainEvent>;
        domainEvents?.Add(domainEvent);
    }
}
