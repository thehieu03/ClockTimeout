using BuildingBlocks.Abstractions;

namespace Payment.Domain.Events;

public sealed record PaymentFailedDomainEvent(
    Guid PaymentId,
    Guid OrderId,
    string ErrorCode,
    string ErrorMessage,
    DateTimeOffset OccurredOn
) : IDomainEvent;
