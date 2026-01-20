namespace Catalog.Application.Models.Results;

public sealed class GetAllProductsResult
{
    #region Fields, Properties and Indexers

    public List<Catalog.Application.Dtos.Products.ProductDto> Items { get; init; }

    #endregion

    #region Ctors

    public GetAllProductsResult(List<Catalog.Application.Dtos.Products.ProductDto> items)
    {
        Items = items;
    }

    #endregion
}
