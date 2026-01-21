using Catalog.Application.Services;
using Catalog.Infrastructure.Data;
using Catalog.Domain.Entities;
using Marten;

namespace Catalog.Infrastructure.Services;

public sealed class SeedDataServices : ISeedDataServices
{
    private const string SystemActor = "System";

    public async Task<bool> SeedDataAsync(IDocumentSession session, CancellationToken cancellationToken)
    {
        try
        {
            // Check if data already exists
            var existingCategories = await session.Query<CategoryEntity>().AnyAsync(cancellationToken);
            var existingBrands = await session.Query<BrandEntity>().AnyAsync(cancellationToken);
            var existingProducts = await session.Query<ProductEntity>().AnyAsync(cancellationToken);

            // Only seed if database is empty
            if (existingCategories || existingBrands || existingProducts)
            {
                return false; // Data already exists
            }

            // Seed Categories first (they are referenced by Products)
            var categories = CategorySeedData.GetCategories(SystemActor);
            foreach (var category in categories)
            {
                session.Store(category);
            }

            // Seed Brands
            var brands = BrandSeedData.GetBrands(SystemActor);
            foreach (var brand in brands)
            {
                session.Store(brand);
            }

            // Save categories and brands first
            await session.SaveChangesAsync(cancellationToken);

            // Seed Products (they reference Categories and Brands)
            var products = ProductSeedData.GetAllProducts(SystemActor);
            foreach (var product in products)
            {
                // Add product images
                ProductSeedData.AddProductImages(product);
                session.Store(product);
            }

            // Save products
            await session.SaveChangesAsync(cancellationToken);

            return true;
        }
        catch (Exception)
        {
            // Log error if needed, but return false to indicate failure
            return false;
        }
    }
}
