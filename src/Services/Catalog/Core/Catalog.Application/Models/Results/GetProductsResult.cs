namespace Catalog.Application.Models.Results;

public sealed class GetProductsResult
{

    #region Fields, Properties and Indexers

    public List<Catalog.Application.Dtos.Products.ProductDto> Items { get; init; }

    public PagingResult Paging { get; init; }

    #endregion

    #region Ctors

    public GetProductsResult(
        List<Catalog.Application.Dtos.Products.ProductDto> items,
        long totalCount,
        PaginationRequest pagination)
    {
        Items = items;
        Paging = PagingResult.Of(totalCount, pagination);
    }

    #endregion

}