namespace Order.Application.Dtos.Orders;

public class ProductDto
{

    #region Fields, Properties and Indexers

    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string ImageUrl { get; set; } = default!;
    public decimal Price { get; set; }

    #endregion

}