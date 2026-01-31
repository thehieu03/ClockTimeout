namespace EventSourcing.Orders;

public sealed record OrderCreatedIntegrationEvent : IntegrationEvent
{
    public Guid OrderId { get; init; }
    public string OrderNo { get; init; } = default!;
    public List<OrderItemIntegrationEvent> OrderItems { get; init; } = default!;
    public decimal TotalPrice { get; init; }
    public decimal FinalPrice { get; init; }
}