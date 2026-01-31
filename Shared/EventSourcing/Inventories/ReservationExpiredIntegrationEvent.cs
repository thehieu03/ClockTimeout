namespace EventSourcing.Inventories;

public sealed record ReservationExpiredIntegrationEvent : IntegrationEvent
{
    public Guid ReservationId { get; init; }
    public Guid OrderId { get; init; }
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = default!;
    public int Quantity { get; init; }
}
