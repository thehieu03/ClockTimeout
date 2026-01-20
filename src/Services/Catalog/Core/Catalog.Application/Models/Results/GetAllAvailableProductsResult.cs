namespace Catalog.Application.Dtos.Filters;

public sealed class GetAllAvailableProductsResult
{
    #region Fields, Properties and Indexers

    public List<ProductDto> Items { get; init; }

    #endregion

    #region Ctors

    public GetAllAvailableProductsResult(List<ProductDto> items)
    {
        Items = items;
    }

    #endregion
}