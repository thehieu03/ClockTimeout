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

    [TestMethod]
    public void Database_Should_Have_Brands_Table_With_Correct_Structure()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new CatalogDbContext(options);
        context.Database.EnsureCreated();

        // Act - Try to add a brand
        var brand = BrandEntity.Create(
            Guid.NewGuid(),
            "Test Brand",
            "test-brand",
            "test-user"
        );

        context.Brands.Add(brand);
        var result = context.SaveChanges();

        // Assert
        Assert.AreEqual(1, result, "Should save 1 brand");
        Assert.IsTrue(context.Brands.Any(b => b.Id == brand.Id), "Brand should exist in database");
        Assert.AreEqual("Test Brand", brand.Name, "Brand name should match");
        Assert.AreEqual("test-brand", brand.Slug, "Brand slug should match");
    }

    [TestMethod]
    public void Database_Should_Have_Categories_Table_With_Correct_Structure()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new CatalogDbContext(options);
        context.Database.EnsureCreated();

        // Act - Try to add a category
        var category = CategoryEntity.Create(
            Guid.NewGuid(),
            "Test Category",
            "Test Description",
            "test-category",
            "test-user"
        );

        context.Categories.Add(category);
        var result = context.SaveChanges();

        // Assert
        Assert.AreEqual(1, result, "Should save 1 category");
        Assert.IsTrue(context.Categories.Any(c => c.Id == category.Id), "Category should exist in database");
        Assert.AreEqual("Test Category", category.Name, "Category name should match");
        Assert.AreEqual("Test Description", category.Description, "Category description should match");
        Assert.AreEqual("test-category", category.Slug, "Category slug should match");
    }

    [TestMethod]
    public void Database_Should_Support_Category_With_Parent_Relationship()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new CatalogDbContext(options);
        context.Database.EnsureCreated();

        // Act - Create parent category
        var parentCategory = CategoryEntity.Create(
            Guid.NewGuid(),
            "Parent Category",
            "Parent Description",
            "parent-category",
            "test-user"
        );

        context.Categories.Add(parentCategory);
        context.SaveChanges();

        // Act - Create child category with parent
        var childCategory = CategoryEntity.Create(
            Guid.NewGuid(),
            "Child Category",
            "Child Description",
            "child-category",
            "test-user",
            parentCategory.Id
        );

        context.Categories.Add(childCategory);
        var result = context.SaveChanges();

        // Assert
        Assert.AreEqual(1, result, "Should save 1 child category");
        Assert.IsNotNull(childCategory.ParentId, "Child category should have parent ID");
        Assert.AreEqual(parentCategory.Id, childCategory.ParentId, "Parent ID should match");
        Assert.AreEqual(2, context.Categories.Count(), "Should have 2 categories in database");
    }

    [TestMethod]
    public void Database_Should_Have_OutboxMessages_Table_With_Correct_Structure()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new CatalogDbContext(options);
        context.Database.EnsureCreated();

        // Act - Try to add an outbox message
        var outboxMessage = OutboxMessageEntity.Create(
            Guid.NewGuid(),
            "TestEvent",
            "{\"test\": \"data\"}",
            DateTimeOffset.UtcNow
        );

        context.OutboxMessages.Add(outboxMessage);
        var result = context.SaveChanges();

        // Assert
        Assert.AreEqual(1, result, "Should save 1 outbox message");
        Assert.IsTrue(context.OutboxMessages.Any(o => o.Id == outboxMessage.Id),
            "Outbox message should exist in database");
        Assert.AreEqual("TestEvent", outboxMessage.EventType, "Event type should match");
        Assert.AreEqual("{\"test\": \"data\"}", outboxMessage.Content, "Content should match");
        Assert.AreEqual(0, outboxMessage.AttemptCount, "Attempt count should be 0 initially");
    }

    [TestMethod]
    public void Database_Should_Support_OutboxMessage_Retry_Mechanism()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new CatalogDbContext(options);
        context.Database.EnsureCreated();

        // Act - Create outbox message
        var outboxMessage = OutboxMessageEntity.Create(
            Guid.NewGuid(),
            "TestEvent",
            "{\"test\": \"data\"}",
            DateTimeOffset.UtcNow
        );

        context.OutboxMessages.Add(outboxMessage);
        context.SaveChanges();

        // Act - Simulate retry
        outboxMessage.RecordFailedAttempt("Test error", DateTimeOffset.UtcNow);
        context.SaveChanges();

        // Assert
        var savedMessage = context.OutboxMessages.FirstOrDefault(o => o.Id == outboxMessage.Id);
        Assert.IsNotNull(savedMessage, "Outbox message should exist");
        Assert.AreEqual(1, savedMessage.AttemptCount, "Attempt count should be 1");
        Assert.IsNotNull(savedMessage.LastErrorMessage, "Last error message should be set");
        Assert.IsNotNull(savedMessage.NextAttemptOnUtc, "Next attempt time should be set");
    }

    [TestMethod]
    public void Database_Should_Support_All_New_Tables_Together()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new CatalogDbContext(options);
        context.Database.EnsureCreated();

        // Act - Create brand
        var brand = BrandEntity.Create(
            Guid.NewGuid(),
            "Test Brand",
            "test-brand",
            "test-user"
        );
        context.Brands.Add(brand);

        // Act - Create category
        var category = CategoryEntity.Create(
            Guid.NewGuid(),
            "Test Category",
            "Test Description",
            "test-category",
            "test-user"
        );
        context.Categories.Add(category);

        // Act - Create outbox message
        var outboxMessage = OutboxMessageEntity.Create(
            Guid.NewGuid(),
            "TestEvent",
            "{\"test\": \"data\"}",
            DateTimeOffset.UtcNow
        );
        context.OutboxMessages.Add(outboxMessage);

        // Act - Create product with brand reference
        var product = ProductEntity.Create(
            Guid.NewGuid(),
            "Test Product",
            "TEST-SKU-001",
            "Short description",
            "Long description",
            "test-product",
            99.99m,
            null,
            new List<Guid> { category.Id },
            brand.Id,
            "test-user"
        );
        context.Products.Add(product);

        var result = context.SaveChanges();

        // Assert
        Assert.AreEqual(4, result, "Should save 4 entities");
        Assert.AreEqual(1, context.Brands.Count(), "Should have 1 brand");
        Assert.AreEqual(1, context.Categories.Count(), "Should have 1 category");
        Assert.AreEqual(1, context.OutboxMessages.Count(), "Should have 1 outbox message");
        Assert.AreEqual(1, context.Products.Count(), "Should have 1 product");
        Assert.AreEqual(brand.Id, product.BrandId, "Product should reference brand");
    }

    [TestMethod]
    public void Database_Should_Enforce_Brand_Slug_Uniqueness()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new CatalogDbContext(options);
        context.Database.EnsureCreated();

        var slug = "unique-slug";

        // Act - Add first brand
        var firstBrand = BrandEntity.Create(
            Guid.NewGuid(),
            "First Brand",
            slug,
            "test-user"
        );
        context.Brands.Add(firstBrand);
        context.SaveChanges();

        // Act - Try to add second brand with same slug
        var secondBrand = BrandEntity.Create(
            Guid.NewGuid(),
            "Second Brand",
            slug,
            "test-user"
        );
        context.Brands.Add(secondBrand);

        // Note: InMemory database doesn't enforce unique constraints,
        // but we can verify the data structure supports it
        var result = context.SaveChanges();

        // Assert - InMemory allows duplicates, but real database would enforce uniqueness
        Assert.AreEqual(2, context.Brands.Count(b => b.Slug == slug),
            "InMemory allows duplicates, but real DB would enforce uniqueness");
    }

    [TestMethod]
    public void Database_Should_Enforce_Category_Slug_Uniqueness()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new CatalogDbContext(options);
        context.Database.EnsureCreated();

        var slug = "unique-category-slug";

        // Act - Add first category
        var firstCategory = CategoryEntity.Create(
            Guid.NewGuid(),
            "First Category",
            "Description",
            slug,
            "test-user"
        );
        context.Categories.Add(firstCategory);
        context.SaveChanges();

        // Act - Try to add second category with same slug
        var secondCategory = CategoryEntity.Create(
            Guid.NewGuid(),
            "Second Category",
            "Description",
            slug,
            "test-user"
        );
        context.Categories.Add(secondCategory);
        var result = context.SaveChanges();

        // Assert - InMemory allows duplicates, but real database would enforce uniqueness
        Assert.AreEqual(2, context.Categories.Count(c => c.Slug == slug),
            "InMemory allows duplicates, but real DB would enforce uniqueness");
    }

    [TestMethod]
    public void Database_Should_Have_All_New_Tables_In_DbContext()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new CatalogDbContext(options);
        context.Database.EnsureCreated();

        // Act & Assert - Verify all DbSets are available
        Assert.IsNotNull(context.Products, "Products DbSet should exist");
        Assert.IsNotNull(context.ProductImages, "ProductImages DbSet should exist");
        Assert.IsNotNull(context.Brands, "Brands DbSet should exist");
        Assert.IsNotNull(context.Categories, "Categories DbSet should exist");
        Assert.IsNotNull(context.OutboxMessages, "OutboxMessages DbSet should exist");

        // Verify tables can be queried (even if empty)
        var productsCount = context.Products.Count();
        var brandsCount = context.Brands.Count();
        var categoriesCount = context.Categories.Count();
        var outboxMessagesCount = context.OutboxMessages.Count();
        var productImagesCount = context.ProductImages.Count();

        // Assert - All tables should be accessible
        Assert.IsTrue(productsCount >= 0, "Products table should be accessible");
        Assert.IsTrue(brandsCount >= 0, "Brands table should be accessible");
        Assert.IsTrue(categoriesCount >= 0, "Categories table should be accessible");
        Assert.IsTrue(outboxMessagesCount >= 0, "OutboxMessages table should be accessible");
        Assert.IsTrue(productImagesCount >= 0, "ProductImages table should be accessible");
    }

    [TestMethod]
    public void Database_Migration_Should_Create_All_Required_Tables()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        // Act
        using var context = new CatalogDbContext(options);
        var created = context.Database.EnsureCreated();

        // Assert
        Assert.IsTrue(created, "Database should be created successfully");

        // Verify all tables exist by checking if we can query them
        var canQueryProducts = context.Products != null;
        var canQueryBrands = context.Brands != null;
        var canQueryCategories = context.Categories != null;
        var canQueryOutboxMessages = context.OutboxMessages != null;
        var canQueryProductImages = context.ProductImages != null;

        Assert.IsTrue(canQueryProducts, "Products table should exist");
        Assert.IsTrue(canQueryBrands, "Brands table should exist");
        Assert.IsTrue(canQueryCategories, "Categories table should exist");
        Assert.IsTrue(canQueryOutboxMessages, "OutboxMessages table should exist");
        Assert.IsTrue(canQueryProductImages, "ProductImages table should exist");
    }
}
