using BuildingBlocks.Abstractions;

namespace Payment.Domain.Events;

public sealed record PaymentCompletedDomainEvent(
    Guid PaymentId,
    Guid OrderId,
    string TransactionId,
    decimal Amount,
    DateTimeOffset OccurredOn
) : IDomainEvent;
