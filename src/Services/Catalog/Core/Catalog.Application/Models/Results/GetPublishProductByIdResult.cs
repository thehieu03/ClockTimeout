using Catalog.Application.Dtos.Products;

namespace Catalog.Application.Models.Results;

public sealed class GetPublishProductByIdResult
{
    #region Fields, Properties and Indexers
    public PublishProductDto Product { get; init; }
    #endregion
    #region ctor
    public GetPublishProductByIdResult(PublishProductDto product)
    {
        Product = product;
    }
    #endregion
}
