namespace EventSourcing.Orders;

public sealed record class OrderDeliveredIntegrationEvent : IntegrationEvent
{
    public Guid OrderId { get; init; }
    public string OrderNo { get; init; } = default!;
    public string Reason { get; init; } = default!;
    public List<OrderItemIntegrationEvent> OrderItems { get; init; } = default!;
}