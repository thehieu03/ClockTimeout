using Order.Domain.Abstractions;
using Order.Domain.Enums;

namespace Order.Domain.Events;

public sealed class OrderCancelledDomainEvent(OrderEntity Order) : IDomainEvent;