namespace Catalog.Application.Models.Results;

public sealed class GetProductByIdResult
{
    #region Fields, Properties and Indexers

    public Catalog.Application.Dtos.Products.ProductDto Product { get; init; }

    #endregion

    #region Ctors

    public GetProductByIdResult(Catalog.Application.Dtos.Products.ProductDto product)
    {
        Product = product;
    }

    #endregion
}
