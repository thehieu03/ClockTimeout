using Catalog.Application.Models.Filters;
using Catalog.Domain.Entities;
using Common.Models;

namespace Catalog.Application.Repositories;

public interface IProductRepository
{

    #region Method

    Task<ProductEntity?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ProductEntity?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<bool> SkuExistsAsync(string sku, Guid? excludeProductId, CancellationToken ct = default);
    Task AddAsync(ProductEntity product, CancellationToken ct = default);
    Task UpdateAsync(ProductEntity product, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<(IReadOnlyList<ProductEntity> Items, long TotalCount)> SearchAsync(
        GetProductsFilter filter, PaginationRequest paging, CancellationToken ct = default);
    Task<IReadOnlyList<ProductEntity>> GetAllAsync(GetAllProductsFilter filter,CancellationToken ct = default);
    Task<IReadOnlyList<ProductEntity>> GetPublishProductsAsync(GetPublishProductsFilter filter,CancellationToken ct = default);

    #endregion

}