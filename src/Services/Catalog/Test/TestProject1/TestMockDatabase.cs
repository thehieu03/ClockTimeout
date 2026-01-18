using Catalog.Application.Models.Filters;
using Catalog.Application.Repositories;
using Catalog.Domain.Entities;
using Catalog.Domain.Enums;
using Common.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace TestProject1;

[TestClass]
public sealed class TestMockDatabase
{
    [TestMethod]
    public async Task Mock_ProductRepository_GetById_Should_Return_Product()
    {
        // Arrange
        var mockRepository = new Mock<IProductRepository>();
        var productId = Guid.NewGuid();
        var expectedProduct = new ProductEntity
        {
            Id = productId,
            Name = "Mock Product",
            Sku = "MOCK-SKU-001",
            Price = 99.99m,
            Published = true,
            Featured = false,
            Status = ProductStatus.InStock,
            CreatedOnUtc = DateTimeOffset.UtcNow,
            LastModifiedOnUtc = DateTimeOffset.UtcNow
        };

        mockRepository
            .Setup(repo => repo.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedProduct);

        // Act
        var result = await mockRepository.Object.GetByIdAsync(productId, CancellationToken.None);

        // Assert
        Assert.IsNotNull(result, "Product should not be null");
        Assert.AreEqual(productId, result.Id, "Product ID should match");
        Assert.AreEqual("Mock Product", result.Name, "Product name should match");
        Assert.AreEqual(99.99m, result.Price, "Product price should match");

        // Verify the method was called
        mockRepository.Verify(repo => repo.GetByIdAsync(productId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task Mock_ProductRepository_GetById_Should_Return_Null_When_Not_Found()
    {
        // Arrange
        var mockRepository = new Mock<IProductRepository>();
        var productId = Guid.NewGuid();

        mockRepository
            .Setup(repo => repo.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductEntity?)null);

        // Act
        var result = await mockRepository.Object.GetByIdAsync(productId, CancellationToken.None);

        // Assert
        Assert.IsNull(result, "Product should be null when not found");
        mockRepository.Verify(repo => repo.GetByIdAsync(productId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task Mock_ProductRepository_GetAll_Should_Return_List_Of_Products()
    {
        // Arrange
        var mockRepository = new Mock<IProductRepository>();
        var filter = new GetAllProductsFilter(null, null);
        var expectedProducts = new List<ProductEntity>
        {
            new ProductEntity
            {
                Id = Guid.NewGuid(),
                Name = "Product 1",
                Sku = "SKU-001",
                Price = 100m,
                Published = true,
                Featured = false,
                Status = ProductStatus.InStock,
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
                Status = ProductStatus.InStock,
                CreatedOnUtc = DateTimeOffset.UtcNow,
                LastModifiedOnUtc = DateTimeOffset.UtcNow
            }
        };

        mockRepository
            .Setup(repo => repo.GetAllAsync(It.IsAny<GetAllProductsFilter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedProducts);

        // Act
        var result = await mockRepository.Object.GetAllAsync(filter, CancellationToken.None);

        // Assert
        Assert.IsNotNull(result, "Products list should not be null");
        Assert.HasCount(2, result, "Should return 2 products");
        Assert.AreEqual("Product 1", result[0].Name, "First product name should match");
        Assert.AreEqual("Product 2", result[1].Name, "Second product name should match");

        mockRepository.Verify(repo => repo.GetAllAsync(It.IsAny<GetAllProductsFilter>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task Mock_ProductRepository_Add_Should_Call_AddAsync()
    {
        // Arrange
        var mockRepository = new Mock<IProductRepository>();
        var product = new ProductEntity
        {
            Id = Guid.NewGuid(),
            Name = "New Product",
            Sku = "NEW-SKU-001",
            Price = 150m,
            Published = false,
            Featured = false,
            Status = ProductStatus.OutOfStock,
            CreatedOnUtc = DateTimeOffset.UtcNow,
            LastModifiedOnUtc = DateTimeOffset.UtcNow
        };

        mockRepository
            .Setup(repo => repo.AddAsync(It.IsAny<ProductEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await mockRepository.Object.AddAsync(product, CancellationToken.None);

        // Assert
        mockRepository.Verify(repo => repo.AddAsync(
            It.Is<ProductEntity>(p => p.Id == product.Id),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task Mock_ProductRepository_Update_Should_Call_UpdateAsync()
    {
        // Arrange
        var mockRepository = new Mock<IProductRepository>();
        var product = new ProductEntity
        {
            Id = Guid.NewGuid(),
            Name = "Updated Product",
            Sku = "UPD-SKU-001",
            Price = 250m,
            Published = true,
            Featured = true,
            Status = ProductStatus.InStock,
            CreatedOnUtc = DateTimeOffset.UtcNow,
            LastModifiedOnUtc = DateTimeOffset.UtcNow
        };

        mockRepository
            .Setup(repo => repo.UpdateAsync(It.IsAny<ProductEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await mockRepository.Object.UpdateAsync(product, CancellationToken.None);

        // Assert
        mockRepository.Verify(repo => repo.UpdateAsync(
            It.Is<ProductEntity>(p => p.Id == product.Id),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task Mock_ProductRepository_Delete_Should_Call_DeleteAsync()
    {
        // Arrange
        var mockRepository = new Mock<IProductRepository>();
        var productId = Guid.NewGuid();

        mockRepository
            .Setup(repo => repo.DeleteAsync(productId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await mockRepository.Object.DeleteAsync(productId, CancellationToken.None);

        // Assert
        mockRepository.Verify(repo => repo.DeleteAsync(
            productId,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task Mock_ProductRepository_GetBySlug_Should_Return_Product()
    {
        // Arrange
        var mockRepository = new Mock<IProductRepository>();
        var slug = "test-product-slug";
        var expectedProduct = new ProductEntity
        {
            Id = Guid.NewGuid(),
            Name = "Test Product",
            Slug = slug,
            Sku = "TEST-SKU-001",
            Price = 99.99m,
            Published = true,
            Featured = false,
            Status = ProductStatus.InStock,
            CreatedOnUtc = DateTimeOffset.UtcNow,
            LastModifiedOnUtc = DateTimeOffset.UtcNow
        };

        mockRepository
            .Setup(repo => repo.GetBySlugAsync(slug, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedProduct);

        // Act
        var result = await mockRepository.Object.GetBySlugAsync(slug, CancellationToken.None);

        // Assert
        Assert.IsNotNull(result, "Product should not be null");
        Assert.AreEqual(slug, result.Slug, "Slug should match");

        mockRepository.Verify(repo => repo.GetBySlugAsync(slug, It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task Mock_ProductRepository_SkuExists_Should_Return_True_When_Sku_Exists()
    {
        // Arrange
        var mockRepository = new Mock<IProductRepository>();
        var sku = "EXISTING-SKU";

        mockRepository
            .Setup(repo => repo.SkuExistsAsync(sku, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await mockRepository.Object.SkuExistsAsync(sku, null, CancellationToken.None);

        // Assert
        Assert.IsTrue(result, "SKU should exist");

        mockRepository.Verify(repo => repo.SkuExistsAsync(sku, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task Mock_ProductRepository_SkuExists_Should_Return_False_When_Sku_Not_Exists()
    {
        // Arrange
        var mockRepository = new Mock<IProductRepository>();
        var sku = "NON-EXISTING-SKU";

        mockRepository
            .Setup(repo => repo.SkuExistsAsync(sku, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await mockRepository.Object.SkuExistsAsync(sku, null, CancellationToken.None);

        // Assert
        Assert.IsFalse(result, "SKU should not exist");

        mockRepository.Verify(repo => repo.SkuExistsAsync(sku, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task Mock_ProductRepository_GetPublishProducts_Should_Return_Only_Published_Products()
    {
        // Arrange
        var mockRepository = new Mock<IProductRepository>();
        var filter = new GetPublishProductsFilter(null);
        var expectedProducts = new List<ProductEntity>
        {
            new ProductEntity
            {
                Id = Guid.NewGuid(),
                Name = "Published Product 1",
                Sku = "PUB-001",
                Price = 100m,
                Published = true,
                Featured = false,
                Status = ProductStatus.InStock,
                CreatedOnUtc = DateTimeOffset.UtcNow,
                LastModifiedOnUtc = DateTimeOffset.UtcNow
            },
            new ProductEntity
            {
                Id = Guid.NewGuid(),
                Name = "Published Product 2",
                Sku = "PUB-002",
                Price = 200m,
                Published = true,
                Featured = true,
                Status = ProductStatus.InStock,
                CreatedOnUtc = DateTimeOffset.UtcNow,
                LastModifiedOnUtc = DateTimeOffset.UtcNow
            }
        };

        mockRepository
            .Setup(repo => repo.GetPublishProductsAsync(It.IsAny<GetPublishProductsFilter>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedProducts);

        // Act
        var result = await mockRepository.Object.GetPublishProductsAsync(filter, CancellationToken.None);

        // Assert
        Assert.IsNotNull(result, "Products list should not be null");
        Assert.HasCount(2, result, "Should return 2 published products");
        Assert.IsTrue(result.All(p => p.Published), "All products should be published");

        mockRepository.Verify(repo => repo.GetPublishProductsAsync(It.IsAny<GetPublishProductsFilter>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task Mock_ProductRepository_Search_Should_Return_Paginated_Results()
    {
        // Arrange
        var mockRepository = new Mock<IProductRepository>();
        var filter = new GetProductsFilter(
            SearchText: null,
            Ids: null,
            BrandId: null,
            CategoryIds: null,
            MinPrice: null,
            MaxPrice: null,
            Published: null,
            Featured: null);
        var paging = new PaginationRequest(1, 10);
        var expectedProducts = new List<ProductEntity>
        {
            new ProductEntity
            {
                Id = Guid.NewGuid(),
                Name = "Search Product 1",
                Sku = "SRCH-001",
                Price = 100m,
                Published = true,
                Featured = false,
                Status = ProductStatus.InStock,
                CreatedOnUtc = DateTimeOffset.UtcNow,
                LastModifiedOnUtc = DateTimeOffset.UtcNow
            }
        };
        var expectedTotalCount = 1L;

        mockRepository
            .Setup(repo => repo.SearchAsync(It.IsAny<GetProductsFilter>(), It.IsAny<PaginationRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((expectedProducts, expectedTotalCount));

        // Act
        var (items, totalCount) = await mockRepository.Object.SearchAsync(filter, paging, CancellationToken.None);

        // Assert
        Assert.IsNotNull(items, "Items should not be null");
        Assert.HasCount(1, items, "Should return 1 product");
        Assert.AreEqual(expectedTotalCount, totalCount, "Total count should match");

        mockRepository.Verify(repo => repo.SearchAsync(
            It.IsAny<GetProductsFilter>(),
            It.IsAny<PaginationRequest>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
