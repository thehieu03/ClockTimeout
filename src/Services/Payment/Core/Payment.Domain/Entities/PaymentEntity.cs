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
    public string? ErrorCode { get; private set; }
    public string? ErrorMessage { get; private set; }
    public string? GatewayResponse { get; private set; }
    public string? RefundReason { get; private set; }
    public string? RefundTransactionId { get; private set; }
    public DateTimeOffset? ProcessedAt { get; private set; }

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

        payment.RaiseDomainEvent(new PaymentCreatedDomainEvent(payment.Id, orderId, amount));
        return payment;
    }

    public void MarkAsProcessing(string? modifiedBy = null)
    {
        if (Status != PaymentStatus.Pending)
            throw new InvalidOperationException($"Cannot process payment in {Status} status");

        Status = PaymentStatus.Processing;
        LastModifiedOnUtc = DateTimeOffset.UtcNow;
        LastModifiedBy = modifiedBy;
    }

    public void SetTransactionId(string transactionId, string? modifiedBy = null)
    {
        if (Status != PaymentStatus.Processing)
            throw new InvalidOperationException($"Cannot set transaction ID for payment in {Status} status");

        TransactionId = transactionId;
        LastModifiedOnUtc = DateTimeOffset.UtcNow;
        LastModifiedBy = modifiedBy;
    }

    public void Complete(string transactionId, string? gatewayResponse = null, string? modifiedBy = null)
    {
        if (Status != PaymentStatus.Processing && Status != PaymentStatus.Pending)
            throw new InvalidOperationException($"Cannot complete payment in {Status} status");

        TransactionId = transactionId;
        GatewayResponse = gatewayResponse;
        Status = PaymentStatus.Completed;
        ProcessedAt = DateTimeOffset.UtcNow;
        LastModifiedOnUtc = DateTimeOffset.UtcNow;
        LastModifiedBy = modifiedBy;

        RaiseDomainEvent(new PaymentCompletedDomainEvent(Id, OrderId, transactionId));
    }

    public void MarkAsFailed(string errorCode, string errorMessage, string? gatewayResponse = null, string? modifiedBy = null)
    {
        if (Status != PaymentStatus.Processing && Status != PaymentStatus.Pending)
            throw new InvalidOperationException($"Cannot fail payment in {Status} status");

        Status = PaymentStatus.Failed;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
        GatewayResponse = gatewayResponse;
        ProcessedAt = DateTimeOffset.UtcNow;
        LastModifiedOnUtc = DateTimeOffset.UtcNow;
        LastModifiedBy = modifiedBy;

        RaiseDomainEvent(new PaymentFailedDomainEvent(Id, OrderId, errorMessage));
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

        RaiseDomainEvent(new PaymentRefundedDomainEvent(Id, OrderId, refundReason));
    }

    public void Cancel(string? modifiedBy = null)
    {
        if (Status != PaymentStatus.Pending)
            throw new InvalidOperationException("Only pending payments can be cancelled.");

        Status = PaymentStatus.Cancelled;
        LastModifiedOnUtc = DateTimeOffset.UtcNow;
        LastModifiedBy = modifiedBy;
    }

    private void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        // Access the protected list from Aggregate base class
        var domainEventsField = typeof(Aggregate<Guid>)
            .GetField("_domainEvents", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var domainEvents = domainEventsField?.GetValue(this) as List<IDomainEvent>;
        domainEvents?.Add(domainEvent);
    }
}
