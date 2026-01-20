using Catalog.Application.Services;
using Catalog.Domain.Entities;
using Catalog.Infrastructure.Data;
using Common.ValueObjects;
using Marten;

namespace Catalog.Infrastructure.Services;

public sealed class SeedDataServices : ISeedDataServices
{
    #region Implementations
    public async Task<bool> SeedDataAsync(IDocumentSession session, CancellationToken cancellationToken)
    {
        var hasChanges = false;
        var performedBy=Actor.System("catalog-services").ToString();
        if (!await session.Query<CategoryEntity>().AnyAsync(cancellationToken)) { 
            hasChanges = true;
            var categories = CategorySeedData.GetCategories(performedBy);
            session.Store(categories);
        }
        if(!await session.Query<BrandEntity>().AnyAsync(cancellationToken))
        {
            hasChanges = true;
            var brands = BrandSeedData.GetBrands(performedBy);
            session.Store(brands);
        }
        var productChanges = await SeedProductDataAsync(session, cancellationToken);
        if (productChanges)
        {
            hasChanges = true;
        }
        if (hasChanges)
        {
            await session.SaveChangesAsync(cancellationToken);
        }
        return hasChanges;

    }

    private async Task<bool> SeedProductDataAsync(IDocumentSession session, CancellationToken cancellationToken)
    {
        var currentCount = await session.Query<ProductEntity>().CountAsync(cancellationToken);
        if (currentCount >= ProductSeedData.TargetProductCount)
        {
            return false;
        }
        var productsToCreate = ProductSeedData.TargetProductCount - currentCount;
        var allProducts = ProductSeedData.GetAllProducts(Actor.System("catalog-serves").ToString());
        var productsToSeed = allProducts.Take(productsToCreate).ToArray();
        foreach (var item in productsToSeed)
        {
            ProductSeedData.AddProductImages(item);
        }
        session.Store(productsToSeed);
        return productsToSeed.Any();
    }
    #endregion
}
