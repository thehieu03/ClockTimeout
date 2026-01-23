using BuildingBlocks.Abstractions;

namespace Payment.Domain.Events;

public record PaymentRefundedDomainEvent(Guid PaymentId, Guid OrderId, string? RefundReason) : IDomainEvent;
