using Order.Domain.Abstractions;
using Order.Domain.Entities;

namespace Order.Domain.Events;

public sealed class OrderCreatedDomainEvent(OrderEntity OrderId) : IDomainEvent;