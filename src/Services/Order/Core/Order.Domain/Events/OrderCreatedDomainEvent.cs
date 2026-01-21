using Order.Domain.Abstractions;

namespace Order.Domain.Events;

public sealed class OrderCreatedDomainEvent(Guid OrderId) : IDomainEvent
{
    public Guid OrderId { get; init; } = OrderId;
    public void Deconstruct(out Guid OrderId)
    {
        OrderId = this.OrderId;
    }
}