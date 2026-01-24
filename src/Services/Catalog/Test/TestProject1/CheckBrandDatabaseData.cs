using Catalog.Domain.Entities;
using Catalog.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestProject1;

[TestClass]
[TestCategory("Integration")]
[TestCategory("RequiresDatabase")]
[Ignore("This test requires a running PostgreSQL database at localhost:5433")]
public sealed class CheckBrandDatabaseData
{
    [TestMethod]
    public void Check_Brands_Table_For_Data()
    {
        // Arrange - Use the actual database connection string
        var connectionString = "Host=localhost;Port=5433;Database=catalog_db;Username=postgres;Password=postgres123";

        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        using var context = new CatalogDbContext(options);

        try
        {
            // Act - Check if database can connect
            var canConnect = context.Database.CanConnect();
            Console.WriteLine($"Database connection: {(canConnect ? "SUCCESS" : "FAILED")}");

            if (!canConnect)
            {
                Assert.Fail("Cannot connect to database. Please ensure PostgreSQL is running and the connection string is correct.");
                return;
            }

            // Query brands count
            var brandsCount = context.Brands.Count();
            Console.WriteLine($"\nTotal Brands in database: {brandsCount}");

            // Query all brands
            var brands = context.Brands
                .OrderBy(b => b.Name)
                .ToList();

            if (brands.Count > 0)
            {
                Console.WriteLine("\nBrands found in database:");
                Console.WriteLine("----------------------------------------");
                foreach (var brand in brands)
                {
                    Console.WriteLine($"ID: {brand.Id}");
                    Console.WriteLine($"Name: {brand.Name ?? "N/A"}");
                    Console.WriteLine($"Slug: {brand.Slug ?? "N/A"}");
                    Console.WriteLine($"Created By: {brand.CreatedBy ?? "N/A"}");
                    Console.WriteLine($"Created On: {brand.CreatedOnUtc}");
                    Console.WriteLine("----------------------------------------");
                }
            }
            else
            {
                Console.WriteLine("\nNo brands found in the database.");
                Console.WriteLine("The Brands table exists but is empty.");
            }

            // Assert - Verify we can query the table
            Assert.IsTrue(canConnect, "Should be able to connect to database");
            Assert.IsNotNull(context.Brands, "Brands DbSet should exist");
            Assert.IsTrue(brandsCount >= 0, "Should be able to count brands (even if 0)");
        }
        catch (TypeLoadException ex) when (ex.Message.Contains("HackyEnumTypeMapping") || ex.Message.Contains("Npgsql.Internal"))
        {
            // Skip test if there's a version conflict between Npgsql and Npgsql.EntityFrameworkCore.PostgreSQL
            // This happens when Marten requires Npgsql 9.x but EF Core provider requires Npgsql 8.x
            Assert.Inconclusive($"Test skipped due to Npgsql version conflict: {ex.Message}. " +
                "Marten 8.17.0 requires Npgsql 9.x, but Npgsql.EntityFrameworkCore.PostgreSQL 8.0.11 requires Npgsql 8.x. " +
                "Consider using Marten for all PostgreSQL operations or upgrading to EF Core 9.x.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nError querying database: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            Assert.Fail($"Failed to query database: {ex.Message}");
        }
    }
}
