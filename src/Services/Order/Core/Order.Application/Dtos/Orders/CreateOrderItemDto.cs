namespace Order.Application.Dtos.Orders;

public class CreateOrderItemDto
{
    #region Fields, Properties and Indexers

    public Guid ProductId { get; set; }

    public string ProductName { get; set; } = default!;

    public string? ProductImageUrl { get; set; }

    public decimal ProductPrice { get; set; }

    public int Quantity { get; set; }

    #endregion
}