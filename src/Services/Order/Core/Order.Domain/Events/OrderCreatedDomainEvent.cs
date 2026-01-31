using Order.Domain.Abstractions;
using Order.Domain.Entities;

namespace Order.Domain.Events;

public sealed record OrderCreatedDomainEvent(OrderEntity OrderId) : IDomainEvent;