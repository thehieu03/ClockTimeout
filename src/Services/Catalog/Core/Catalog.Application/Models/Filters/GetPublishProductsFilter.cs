namespace Catalog.Application.Models.Filters;

public class GetPublishProductsFilter
{
    public string? SearchText { get; set; }
    public Guid? BrandId { get; set; }
    public List<Guid>? CategoryIds { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool? Published { get; set; }
}