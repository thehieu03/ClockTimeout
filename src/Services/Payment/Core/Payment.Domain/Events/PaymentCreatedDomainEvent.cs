using BuildingBlocks.Abstractions;

namespace Payment.Domain.Events;

public record PaymentCreatedDomainEvent(Guid PaymentId, Guid OrderId, decimal Amount) : IDomainEvent;
