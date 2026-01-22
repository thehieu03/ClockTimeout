namespace Order.Application.Dtos.Orders;

public class OrderItemDto
{
    #region Fields, Properties and Indexers

    public Guid Id { get; set; }

    public ProductDto Product { get; set; } = default!;

    public int Quantity { get; set; }

    public decimal LineTotal { get; set; }

    #endregion
}