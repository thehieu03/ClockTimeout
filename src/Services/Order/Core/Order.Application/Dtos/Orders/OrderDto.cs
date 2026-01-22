namespace Order.Application.Dtos.Orders;

public class OrderDto
{
    #region Fields, Properties and Indexers

    public Guid Id { get; set; }

    public string OrderNo { get; set; } = default!;

    public CustomerDto Customer { get; set; } = default!;

    public AddressDto ShippingAddress { get; set; } = default!;

    public List<OrderItemDto> OrderItems { get; set; } = new();

    public int Status { get; set; }

    public string StatusName { get; set; } = default!;

    public decimal TotalPrice { get; set; }

    public decimal FinalPrice { get; set; }

    public string? CouponCode { get; set; }

    public decimal DiscountAmount { get; set; }

    public string? Notes { get; set; }

    public DateTimeOffset CreatedOnUtc { get; set; }

    public string? CreatedBy { get; set; }

    #endregion
}