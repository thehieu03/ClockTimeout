using Catalog.Application.Models.Filters;
using Catalog.Application.Repositories;
using Catalog.Domain.Entities;
using Marten;
using Marten.Pagination;
using Microsoft.Extensions.Logging;

namespace Catalog.Infrastructure.Repositories;

public class ProductRepository(IDocumentSession session, ILogger<ProductRepository> logger) : IProductRepository
{

    public async Task<ProductEntity?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        logger.LogDebug("Getting product by ID: {ProductId}", id);
        var product = await session.LoadAsync<ProductEntity>(id, ct);
        if (product == null)
        {
            logger.LogWarning("Product with ID: {ProductId} not found", id);
            return null;
        }
        logger.LogDebug("Successfully retrieved product: {ProductId}", id);
        return product;
    }
    public async Task<ProductEntity?> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            logger.LogWarning("Slug is null or empty");
            return null;
        }
        logger.LogDebug("Getting product by slug: {Slug}", slug);
        var product = await session.Query<ProductEntity>()
            .FirstOrDefaultAsync(x => x.Slug == slug, ct);
        if (product == null)
        {
            logger.LogWarning("Product with slug: {Slug} not found", slug);
            return null;
        }
        logger.LogDebug("Successfully retrieved product: {Slug}", slug);
        return product;
    }
    public async Task<bool> SkuExistsAsync(string sku, Guid? excludeProductId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(sku))
        {
            logger.LogWarning("Sku is null or empty");
            return false;
        }
        logger.LogDebug("Checking if sku exits: {Sku}, excluding product: {ExcludeProductId}", sku, excludeProductId);
        var query = session
            .Query<ProductEntity>()
            .Where(x => x.Sku == sku);
        if (excludeProductId.HasValue)
        {
            query = query.Where(x => x.Id != excludeProductId.Value);
        }
        var exists = await query.AnyAsync(ct);
        logger.LogDebug("Sku {Sku} exits: {SkuExists}", sku, exists);
        return exists;
    }
    public async Task AddAsync(ProductEntity product, CancellationToken ct = default)
    {
        if (product == null)
        {
            logger.LogError("Cannot add null product entity");
            throw new ArgumentNullException(nameof(product));
        }
        logger.LogDebug("Adding product: {ProductId}, Name: {ProductName}, Sku: {Sku}", product.Id, product.Name, product.Sku);
        session.Store(product);
        await session.SaveChangesAsync(ct);
        logger.LogInformation("Successfully added product: {ProductId} with Sku {Sku}", product.Id, product.Sku);

    }
    public async Task UpdateAsync(ProductEntity product, CancellationToken ct = default)
    {
        if (product == null)
        {
            logger.LogError("Cannot update null product entity");
            throw new ArgumentNullException(nameof(product));
        }
        logger.LogDebug("Updating product: {ProductId}, Name: {ProductName}, Sku: {Sku}", product.Id, product.Name, product.Sku);
        session.Store(product);
        await session.SaveChangesAsync(ct);
        logger.LogInformation("Successfully updated product: {ProductId} with Sku {Sku}", product.Id, product.Sku);

    }
    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        logger.LogDebug("Deleting product: {ProductId}", id);
        session.Delete<ProductEntity>(id);
        await session.SaveChangesAsync(ct);
        logger.LogInformation("Successfully deleted product: {ProductId}", id);
    }
    public async Task<(IReadOnlyList<ProductEntity> Items, long TotalCount)> SearchAsync(GetProductsFilter filter, PaginationRequest paging,
        CancellationToken ct = default)
    {
        var query = session.Query<ProductEntity>().AsQueryable();
        if (!string.IsNullOrWhiteSpace(filter.SearchText))
        {
            var searchText = filter.SearchText.Trim();
            query = query.Where(x => x.Name != null && x.Name.Contains(searchText));
        }
        if (filter.Ids?.Length > 0)
        {
            query = query.Where(x => filter.Ids.Contains(x.Id));
        }
        var totalCount = await query.CountAsync(ct);
        var pagedResult = await query
            .OrderByDescending(x => x.CreatedOnUtc)
            .ToPagedListAsync(paging.PageNumber, paging.PageSize, ct);
        var items = pagedResult.ToList().AsReadOnly();
        logger.LogInformation("Search completed: Found {TotalCount} total products, returning {ItemCount} items",
            totalCount, items.Count);
        return (items, totalCount);
    }
    public async Task<IReadOnlyList<ProductEntity>> GetAllAsync(GetAllProductsFilter filter, CancellationToken ct = default)
    {
        logger.LogDebug("Getting all products with filter: SearchText={SearchText}, Ids={Ids}", filter.SearchText, filter.Ids?.Length ?? 0);
        var query = session.Query<ProductEntity>().AsQueryable();
        if (!string.IsNullOrWhiteSpace(filter.SearchText))
        {
            var searchText = filter.SearchText.Trim();
            query = query.Where(x => x.Name != null && x.Name.Contains(searchText));
        }
        // Apply IDs filter
        if (filter.Ids?.Length > 0)
        {
            query = query.Where(x => filter.Ids.Contains(x.Id));
        }
        var products = await query
            .OrderByDescending(x => x.CreatedOnUtc)
            .ToListAsync(ct);
        logger.LogDebug("Successfully retrieved {ItemCount} products", products.Count);
        return products;
    }
    public async Task<IReadOnlyList<ProductEntity>> GetPublishProductsAsync(GetPublishProductsFilter filter, CancellationToken ct = default)
    {
        logger.LogDebug("Getting published products with filter: SearchText= {SearchText}", filter.SearchText);
        var query = session.Query<ProductEntity>().Where(x => x.Published).AsQueryable();
        // Apply search text filter
        if (!string.IsNullOrWhiteSpace(filter.SearchText))
        {
            var searchText = filter.SearchText.Trim();
            query = query.Where(x => x.Name != null && x.Name.Contains(searchText));
        }
        var products = await query.OrderByDescending(x=>x.CreatedOnUtc).ToListAsync(ct);
        logger.LogDebug("Successfully retrieved {ItemCount} products", products.Count);
        return products;  
    }
}