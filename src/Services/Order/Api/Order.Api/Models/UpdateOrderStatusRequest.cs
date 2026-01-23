using Order.Domain.Enums;

namespace Order.Api.Models;

public sealed class UpdateOrderStatusRequest
{
    public OrderStatus Status { get; set; }
    public string? Reason { get; set; }
}
