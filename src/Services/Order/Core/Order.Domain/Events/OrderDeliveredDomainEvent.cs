using Order.Domain.Abstractions;
using Order.Domain.Enums;

namespace Order.Domain.Events;

public sealed record OrderDeliveredDomainEvent(OrderEntity Order) : IDomainEvent;