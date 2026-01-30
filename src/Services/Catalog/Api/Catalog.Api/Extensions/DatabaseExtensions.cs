using Catalog.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Api.Extensions;

public static class DatabaseExtensions
{
    public static async Task InitialiseDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        try
        {
            await context.Database.MigrateAsync();

            await SeedAsync(context, logger);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initialising the database.");
            throw;
        }
    }

    private static async Task SeedAsync(CatalogDbContext context, ILogger<Program> logger)
    {
        if (!await context.Brands.AnyAsync())
        {
            logger.LogInformation("Seeding Brands...");
            await context.Brands.AddRangeAsync(BrandSeedData.GetBrands("InitialSeeding"));
            await context.SaveChangesAsync();
        }

        if (!await context.Categories.AnyAsync())
        {
            logger.LogInformation("Seeding Categories...");
            await context.Categories.AddRangeAsync(CategorySeedData.GetCategories("InitialSeeding"));
            await context.SaveChangesAsync();
        }

        if (!await context.Products.AnyAsync())
        {
            logger.LogInformation("Seeding Products...");
            var products = ProductSeedData.GetAllProducts("InitialSeeding");
            
            // Note: ProductSeedData already sets CategoryIds and BrandId on the entities.
            // Since we just seeded Brands and Categories, the IDs should match if they are deterministic (which they are in SeedData classes).
            
            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();
            
            // Add Product Images
            foreach (var product in products)
            {
               ProductSeedData.AddProductImages(product);
            }
             await context.SaveChangesAsync();
        }
    }
}
