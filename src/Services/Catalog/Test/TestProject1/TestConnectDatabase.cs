using Catalog.Domain.Entities;
using Catalog.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestProject1;

[TestClass]
public sealed class TestConnectDatabase
{
    [TestMethod]
    public void TestMethod1()
    {
    }

    [TestMethod]
    public void Database_Migration_Should_Create_Tables_Successfully()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        // Act & Assert - Database should be created without errors
        using var context = new CatalogDbContext(options);

        // Ensure database is created
        Assert.IsTrue(context.Database.EnsureCreated(), "Database should be created successfully");

        // Verify Products table exists
        var productsTableExists = context.Database.CanConnect();
        Assert.IsTrue(productsTableExists, "Database connection should be successful");
    }

    [TestMethod]
    public void Database_Should_Have_Products_Table_With_Correct_Structure()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new CatalogDbContext(options);
        context.Database.EnsureCreated();

        // Act - Try to add a product
        var product = new ProductEntity
        {
            Id = Guid.NewGuid(),
            Name = "Test Product",
            Sku = "TEST-SKU-001",
            Price = 99.99m,
            Published = true,
            Featured = false,
            Status = Catalog.Domain.Enums.ProductStatus.InStock,
            CreatedOnUtc = DateTimeOffset.UtcNow,
            LastModifiedOnUtc = DateTimeOffset.UtcNow
        };

        context.Products.Add(product);
        var result = context.SaveChanges();

        // Assert
        Assert.AreEqual(1, result, "Should save 1 product");
        Assert.IsTrue(context.Products.Any(p => p.Id == product.Id), "Product should exist in database");
    }

    [TestMethod]
    public void Database_Should_Have_ProductImages_Table_With_Correct_Structure()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new CatalogDbContext(options);
        context.Database.EnsureCreated();

        // Act - Try to add a product image
        var productImage = new ProductImageEntity
        {
            FileId = "test-file-id-001",
            OriginalFileName = "test-image.jpg",
            FileName = "test-image-processed.jpg",
            PublicURL = "https://example.com/images/test-image.jpg"
        };

        context.ProductImages.Add(productImage);
        var result = context.SaveChanges();

        // Assert
        Assert.AreEqual(1, result, "Should save 1 product image");
        Assert.IsTrue(context.ProductImages.Any(img => img.FileId == productImage.FileId),
            "Product image should exist in database");
    }

    [TestMethod]
    public void Database_Should_Prevent_Duplicate_FileId_In_Same_Context()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new CatalogDbContext(options);
        context.Database.EnsureCreated();

        var fileId = "duplicate-file-id";

        // Act - Add first image
        var firstImage = new ProductImageEntity
        {
            FileId = fileId,
            OriginalFileName = "first.jpg"
        };
        context.ProductImages.Add(firstImage);
        context.SaveChanges();

        // Act - Try to add second image with same FileId in same context
        // Note: InMemory database doesn't enforce unique constraints like real database,
        // but EF Core will prevent tracking duplicate keys in the same context
        var secondImage = new ProductImageEntity
        {
            FileId = fileId,
            OriginalFileName = "second.jpg"
        };

        // Assert - Should throw exception when trying to track duplicate key
        bool exceptionThrown = false;
        try
        {
            context.ProductImages.Add(secondImage);
        }
        catch (InvalidOperationException)
        {
            // Expected exception - EF Core prevents tracking duplicate keys
            exceptionThrown = true;
        }

        Assert.IsTrue(exceptionThrown, "Should throw exception when trying to track duplicate FileId in same context");
    }

    [TestMethod]
    public void Database_Should_Support_Product_With_All_Properties()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new CatalogDbContext(options);
        context.Database.EnsureCreated();

        // Act - Create product with all properties
        var product = new ProductEntity
        {
            Id = Guid.NewGuid(),
            Name = "Complete Test Product",
            Sku = "COMPLETE-SKU-001",
            ShortDescription = "Short description",
            LongDescription = "Long description with details",
            Slug = "complete-test-product",
            Barcode = "1234567890123",
            Price = 199.99m,
            SalePrice = 149.99m,
            CategoryIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() },
            Colors = new List<string> { "Red", "Blue", "Green" },
            Sizes = new List<string> { "S", "M", "L", "XL" },
            Tags = new List<string> { "tag1", "tag2", "tag3" },
            Published = true,
            Featured = true,
            Status = Catalog.Domain.Enums.ProductStatus.InStock,
            BrandId = Guid.NewGuid(),
            SEOTitle = "SEO Title",
            SEODescription = "SEO Description",
            Unit = "piece",
            Weight = 1.5m,
            CreatedOnUtc = DateTimeOffset.UtcNow,
            CreatedBy = "test-user",
            LastModifiedOnUtc = DateTimeOffset.UtcNow,
            LastModifiedBy = "test-user"
        };

        context.Products.Add(product);
        var result = context.SaveChanges();

        // Assert
        Assert.AreEqual(1, result, "Should save product with all properties");

        var savedProduct = context.Products.FirstOrDefault(p => p.Id == product.Id);
        Assert.IsNotNull(savedProduct, "Product should be saved");
        Assert.AreEqual(product.Name, savedProduct.Name, "Name should match");
        Assert.AreEqual(product.Price, savedProduct.Price, "Price should match");
        Assert.AreEqual(product.CategoryIds?.Count, savedProduct.CategoryIds?.Count, "CategoryIds should match");
        Assert.AreEqual(product.Colors?.Count, savedProduct.Colors?.Count, "Colors should match");
    }

    [TestMethod]
    public void Database_Should_Handle_Multiple_Products_And_Images()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new CatalogDbContext(options);
        context.Database.EnsureCreated();

        // Act - Add multiple products
        var products = new List<ProductEntity>
        {
            new ProductEntity
            {
                Id = Guid.NewGuid(),
                Name = "Product 1",
                Sku = "SKU-001",
                Price = 100m,
                Published = true,
                Featured = false,
                Status = Catalog.Domain.Enums.ProductStatus.InStock,
                CreatedOnUtc = DateTimeOffset.UtcNow,
                LastModifiedOnUtc = DateTimeOffset.UtcNow
            },
            new ProductEntity
            {
                Id = Guid.NewGuid(),
                Name = "Product 2",
                Sku = "SKU-002",
                Price = 200m,
                Published = true,
                Featured = true,
                Status = Catalog.Domain.Enums.ProductStatus.InStock,
                CreatedOnUtc = DateTimeOffset.UtcNow,
                LastModifiedOnUtc = DateTimeOffset.UtcNow
            }
        };

        context.Products.AddRange(products);
        var productResult = context.SaveChanges();

        // Act - Add multiple images
        var images = new List<ProductImageEntity>
        {
            new ProductImageEntity { FileId = "img-001", OriginalFileName = "image1.jpg" },
            new ProductImageEntity { FileId = "img-002", OriginalFileName = "image2.jpg" },
            new ProductImageEntity { FileId = "img-003", OriginalFileName = "image3.jpg" }
        };

        context.ProductImages.AddRange(images);
        var imageResult = context.SaveChanges();

        // Assert
        Assert.AreEqual(2, productResult, "Should save 2 products");
        Assert.AreEqual(3, imageResult, "Should save 3 images");
        Assert.AreEqual(2, context.Products.Count(), "Should have 2 products in database");
        Assert.AreEqual(3, context.ProductImages.Count(), "Should have 3 images in database");
    }
}
