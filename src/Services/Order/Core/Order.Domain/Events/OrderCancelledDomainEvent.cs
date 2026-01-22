using Order.Domain.Abstractions;
using Order.Domain.Entities;

namespace Order.Domain.Events;

public sealed class OrderCancelledDomainEvent(OrderEntity Order) : IDomainEvent;