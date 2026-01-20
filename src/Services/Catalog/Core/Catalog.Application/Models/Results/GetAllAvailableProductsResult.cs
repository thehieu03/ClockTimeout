namespace Catalog.Application.Models.Filters;

public sealed class GetAllAvailableProductsResult
{
    #region Fields, Properties and Indexers

    public List<Dtos.Products.ProductDto> Items { get; init; }

    #endregion

    #region Ctors

    public GetAllAvailableProductsResult(List<Dtos.Products.ProductDto> items)
    {
        Items = items;
    }

    #endregion
}