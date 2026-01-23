using BuildingBlocks.Abstractions;

namespace Payment.Domain.Events;

public record PaymentCompletedDomainEvent(Guid PaymentId, Guid OrderId, string TransactionId) : IDomainEvent;
