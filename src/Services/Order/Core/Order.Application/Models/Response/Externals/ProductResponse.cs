namespace Order.Application.Models.Response.Externals;

public class ProductResponse
{

    #region Fields, Properties and Indexers

    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public decimal Price { get; set; }
    public string Thumbnail { get; set; } = default!;

    #endregion
}