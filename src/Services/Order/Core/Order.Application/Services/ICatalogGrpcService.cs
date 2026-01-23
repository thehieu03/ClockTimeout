using Order.Application.Models.Response.Externals;

namespace Order.Application.Services;

public interface ICatalogGrpcService
{

    #region Methods

    Task<ProductResponse?> GetProductByIdAsync(string productId,CancellationToken cancellationToken = default);
    Task<GetAllProductsResponse?> GetAllProductsAsync(string[]? ids=null,string searchText="",CancellationToken cancellationToken = default);
    Task<GetAllProductsResponse?> GetAllAvailableProductsAsync(string[]? ids=null,string searchText="",CancellationToken cancellationToken = default);
    #endregion
}