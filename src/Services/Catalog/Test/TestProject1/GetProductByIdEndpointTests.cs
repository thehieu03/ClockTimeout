using Catalog.Api.Endpoints;
using Catalog.Application.Dtos.Products;
using Catalog.Application.Features.Product.Queries;
using Catalog.Application.Models.Results;
using Catalog.Domain.Enums;
using Common.Models.Reponses;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Reflection;

namespace TestProject1;

[TestClass]
public sealed class GetProductByIdEndpointTests
{
    private Mock<ISender> _mockSender = null!;
    private GetProductById _endpoint = null!;
    private MethodInfo _handlerMethod = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockSender = new Mock<ISender>();
        _endpoint = new GetProductById();

        // Get private handler method using reflection
        _handlerMethod = typeof(GetProductById).GetMethod(
            "HandleGetProductAsync",
            BindingFlags.NonPublic | BindingFlags.Instance)!;
    }

    [TestMethod]
    public async Task HandleGetProductAsync_WithValidProductId_ShouldReturnApiGetResponse()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;

        var productDto = new ProductDto
        {
            Id = productId,
            Name = "iPhone 15 Pro",
            Sku = "IPHONE-15-PRO-001",
            ShortDescription = "Latest iPhone with A17 Pro chip",
            LongDescription = "The iPhone 15 Pro features a 6.1-inch Super Retina XDR display",
            Slug = "iphone-15-pro",
            Price = 999.99m,
            SalePrice = 899.99m,
            CategoryNames = new List<string> { "Electronics", "Phones" },
            CategoryIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() },
            BrandName = "Apple",
            BrandId = Guid.NewGuid(),
            Status = ProductStatus.InStock,
            Published = true,
            Featured = false
        };

        var expectedResult = new GetProductByIdResult(productDto);

        _mockSender.Setup(x => x.Send(
            It.Is<GetProductByIdQuery>(q => q.ProductId == productId),
            cancellationToken))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await InvokeHandlerAsync(productId, cancellationToken);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Result);
        Assert.IsNotNull(result.Result.Product);
        Assert.AreEqual(productId, result.Result.Product.Id);
        Assert.AreEqual("iPhone 15 Pro", result.Result.Product.Name);
        Assert.AreEqual("IPHONE-15-PRO-001", result.Result.Product.Sku);
        Assert.AreEqual(999.99m, result.Result.Product.Price);
        Assert.AreEqual(2, result.Result.Product.CategoryNames?.Count);
        Assert.IsNotNull(result.Result.Product.BrandName);
        Assert.AreEqual("Apple", result.Result.Product.BrandName);

        _mockSender.Verify(x => x.Send(
            It.Is<GetProductByIdQuery>(q => q.ProductId == productId),
            cancellationToken), Times.Once);
    }

    [TestMethod]
    public async Task HandleGetProductAsync_WithProductWithoutCategories_ShouldReturnProductWithoutCategories()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;

        var productDto = new ProductDto
        {
            Id = productId,
            Name = "Test Product",
            Sku = "TEST-001",
            Price = 100m,
            CategoryNames = null,
            CategoryIds = null,
            BrandName = null,
            BrandId = null
        };

        var expectedResult = new GetProductByIdResult(productDto);

        _mockSender.Setup(x => x.Send(
            It.IsAny<GetProductByIdQuery>(),
            cancellationToken))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await InvokeHandlerAsync(productId, cancellationToken);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Result);
        Assert.IsNotNull(result.Result.Product);
        Assert.IsNull(result.Result.Product.CategoryNames);
        Assert.IsNull(result.Result.Product.CategoryIds);
        Assert.IsNull(result.Result.Product.BrandName);
    }

    [TestMethod]
    public async Task HandleGetProductAsync_WithProductWithSingleCategory_ShouldReturnProductWithCategory()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;

        var productDto = new ProductDto
        {
            Id = productId,
            Name = "Test Product",
            Sku = "TEST-001",
            Price = 100m,
            CategoryNames = new List<string> { "Electronics" },
            CategoryIds = new List<Guid> { categoryId },
            BrandName = "Test Brand",
            BrandId = Guid.NewGuid()
        };

        var expectedResult = new GetProductByIdResult(productDto);

        _mockSender.Setup(x => x.Send(
            It.IsAny<GetProductByIdQuery>(),
            cancellationToken))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await InvokeHandlerAsync(productId, cancellationToken);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Result.Product);
        Assert.AreEqual(1, result.Result.Product.CategoryNames?.Count);
        Assert.AreEqual("Electronics", result.Result.Product.CategoryNames?[0]);
        Assert.AreEqual(1, result.Result.Product.CategoryIds?.Count);
        Assert.AreEqual(categoryId, result.Result.Product.CategoryIds?[0]);
    }

    [TestMethod]
    public async Task HandleGetProductAsync_WithDifferentProductIds_ShouldCallSenderWithCorrectId()
    {
        // Arrange
        var productId1 = Guid.NewGuid();
        var productId2 = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;

        var productDto1 = new ProductDto { Id = productId1, Name = "Product 1", Sku = "PROD-001", Price = 100m };
        var productDto2 = new ProductDto { Id = productId2, Name = "Product 2", Sku = "PROD-002", Price = 200m };

        _mockSender.Setup(x => x.Send(
            It.Is<GetProductByIdQuery>(q => q.ProductId == productId1),
            cancellationToken))
            .ReturnsAsync(new GetProductByIdResult(productDto1));

        _mockSender.Setup(x => x.Send(
            It.Is<GetProductByIdQuery>(q => q.ProductId == productId2),
            cancellationToken))
            .ReturnsAsync(new GetProductByIdResult(productDto2));

        // Act
        var result1 = await InvokeHandlerAsync(productId1, cancellationToken);
        var result2 = await InvokeHandlerAsync(productId2, cancellationToken);

        // Assert
        Assert.IsNotNull(result1);
        Assert.IsNotNull(result2);
        Assert.AreEqual(productId1, result1.Result.Product.Id);
        Assert.AreEqual(productId2, result2.Result.Product.Id);
        Assert.AreEqual("Product 1", result1.Result.Product.Name);
        Assert.AreEqual("Product 2", result2.Result.Product.Name);

        _mockSender.Verify(x => x.Send(
            It.Is<GetProductByIdQuery>(q => q.ProductId == productId1),
            cancellationToken), Times.Once);

        _mockSender.Verify(x => x.Send(
            It.Is<GetProductByIdQuery>(q => q.ProductId == productId2),
            cancellationToken), Times.Once);
    }

    [TestMethod]
    public async Task HandleGetProductAsync_ShouldPassCancellationToken()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        var productDto = new ProductDto
        {
            Id = productId,
            Name = "Test Product",
            Sku = "TEST-001",
            Price = 100m
        };

        var expectedResult = new GetProductByIdResult(productDto);

        _mockSender.Setup(x => x.Send(
            It.IsAny<GetProductByIdQuery>(),
            cancellationToken))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await InvokeHandlerAsync(productId, cancellationToken);

        // Assert
        Assert.IsNotNull(result);
        _mockSender.Verify(x => x.Send(
            It.IsAny<GetProductByIdQuery>(),
            cancellationToken), Times.Once);
    }

    private async Task<ApiGetResponse<GetProductByIdResult>> InvokeHandlerAsync(
        Guid productId,
        CancellationToken cancellationToken)
    {
        return await (Task<ApiGetResponse<GetProductByIdResult>>)_handlerMethod.Invoke(
            _endpoint,
            new object[]
            {
                _mockSender.Object,
                productId,
                cancellationToken
            })!;
    }
}
