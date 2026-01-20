using Catalog.Api.Endpoints;
using Catalog.Application.Dtos.Products;
using Catalog.Application.Features.Product.Queries;
using Catalog.Application.Models.Filters;
using Common.Models;
using Common.Models.Reponses;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Reflection;

namespace TestProject1;

[TestClass]
public sealed class GetProductsEndpointTests
{
    private Mock<ISender> _mockSender = null!;
    private GetProducts _endpoint = null!;
    private MethodInfo _handlerMethod = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockSender = new Mock<ISender>();
        _endpoint = new GetProducts();

        // Get private handler method using reflection
        _handlerMethod = typeof(GetProducts).GetMethod(
            "HandleGetProductsAsync",
            BindingFlags.NonPublic | BindingFlags.Instance)!;
    }

    [TestMethod]
    public async Task HandleGetProductsAsync_WithValidFilter_ShouldReturnApiGetResponse()
    {
        // Arrange
        var filter = new GetProductsFilter(SearchText: "iPhone", Ids: null);
        var pagination = new PaginationRequest(PageNumber: 1, PageSize: 10);
        var cancellationToken = CancellationToken.None;

        var products = new List<ProductDto>
        {
            new ProductDto
            {
                Id = Guid.NewGuid(),
                Name = "iPhone 15 Pro",
                Sku = "IPHONE-15-PRO-001",
                Price = 999.99m,
                CategoryNames = new List<string> { "Electronics" },
                CategoryIds = new List<Guid> { Guid.NewGuid() }
            },
            new ProductDto
            {
                Id = Guid.NewGuid(),
                Name = "iPhone 15",
                Sku = "IPHONE-15-001",
                Price = 799.99m,
                CategoryNames = new List<string> { "Electronics" },
                CategoryIds = new List<Guid> { Guid.NewGuid() }
            }
        };

        var expectedResult = new GetProductsResult(products, totalCount: 2, pagination);

        _mockSender.Setup(x => x.Send(
            It.Is<GetAllProductsQuery>(q =>
                q.filter.SearchText == filter.SearchText &&
                q.pagination.PageNumber == pagination.PageNumber &&
                q.pagination.PageSize == pagination.PageSize),
            cancellationToken))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await InvokeHandlerAsync(filter, pagination, cancellationToken);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Result);
        Assert.AreEqual(2, result.Result.Items.Count);
        Assert.AreEqual(2, result.Result.Paging.TotalCount);
        Assert.AreEqual("iPhone 15 Pro", result.Result.Items[0].Name);

        _mockSender.Verify(x => x.Send(
            It.Is<GetAllProductsQuery>(q =>
                q.filter.SearchText == filter.SearchText &&
                q.pagination.PageNumber == pagination.PageNumber &&
                q.pagination.PageSize == pagination.PageSize),
            cancellationToken), Times.Once);
    }

    [TestMethod]
    public async Task HandleGetProductsAsync_WithIdsFilter_ShouldReturnFilteredProducts()
    {
        // Arrange
        var productId1 = Guid.NewGuid();
        var productId2 = Guid.NewGuid();
        var filter = new GetProductsFilter(SearchText: null, Ids: new[] { productId1, productId2 });
        var pagination = new PaginationRequest(PageNumber: 1, PageSize: 10);
        var cancellationToken = CancellationToken.None;

        var products = new List<ProductDto>
        {
            new ProductDto
            {
                Id = productId1,
                Name = "Product 1",
                Sku = "PROD-001",
                Price = 100m
            },
            new ProductDto
            {
                Id = productId2,
                Name = "Product 2",
                Sku = "PROD-002",
                Price = 200m
            }
        };

        var expectedResult = new GetProductsResult(products, totalCount: 2, pagination);

        _mockSender.Setup(x => x.Send(
            It.Is<GetAllProductsQuery>(q =>
                q.filter.Ids != null &&
                q.filter.Ids.Length == 2 &&
                q.filter.Ids.Contains(productId1) &&
                q.filter.Ids.Contains(productId2)),
            cancellationToken))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await InvokeHandlerAsync(filter, pagination, cancellationToken);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Result);
        Assert.AreEqual(2, result.Result.Items.Count);
        Assert.IsTrue(result.Result.Items.Any(p => p.Id == productId1));
        Assert.IsTrue(result.Result.Items.Any(p => p.Id == productId2));
    }

    [TestMethod]
    public async Task HandleGetProductsAsync_WithEmptyResult_ShouldReturnEmptyList()
    {
        // Arrange
        var filter = new GetProductsFilter(SearchText: "NonExistentProduct", Ids: null);
        var pagination = new PaginationRequest(PageNumber: 1, PageSize: 10);
        var cancellationToken = CancellationToken.None;

        var expectedResult = new GetProductsResult(new List<ProductDto>(), totalCount: 0, pagination);

        _mockSender.Setup(x => x.Send(
            It.IsAny<GetAllProductsQuery>(),
            cancellationToken))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await InvokeHandlerAsync(filter, pagination, cancellationToken);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Result);
        Assert.AreEqual(0, result.Result.Items.Count);
        Assert.AreEqual(0, result.Result.Paging.TotalCount);
    }

    [TestMethod]
    public async Task HandleGetProductsAsync_WithPagination_ShouldPassCorrectPaginationParameters()
    {
        // Arrange
        var filter = new GetProductsFilter(SearchText: null, Ids: null);
        var pagination = new PaginationRequest(PageNumber: 2, PageSize: 5);
        var cancellationToken = CancellationToken.None;

        var expectedResult = new GetProductsResult(new List<ProductDto>(), totalCount: 0, pagination);

        _mockSender.Setup(x => x.Send(
            It.Is<GetAllProductsQuery>(q =>
                q.pagination.PageNumber == 2 &&
                q.pagination.PageSize == 5),
            cancellationToken))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await InvokeHandlerAsync(filter, pagination, cancellationToken);

        // Assert
        Assert.IsNotNull(result);
        _mockSender.Verify(x => x.Send(
            It.Is<GetAllProductsQuery>(q =>
                q.pagination.PageNumber == 2 &&
                q.pagination.PageSize == 5),
            cancellationToken), Times.Once);
    }

    [TestMethod]
    public async Task HandleGetProductsAsync_WithNullFilter_ShouldHandleGracefully()
    {
        // Arrange
        var filter = new GetProductsFilter(SearchText: null, Ids: null);
        var pagination = new PaginationRequest(PageNumber: 1, PageSize: 10);
        var cancellationToken = CancellationToken.None;

        var expectedResult = new GetProductsResult(new List<ProductDto>(), totalCount: 0, pagination);

        _mockSender.Setup(x => x.Send(
            It.Is<GetAllProductsQuery>(q =>
                q.filter.SearchText == null &&
                q.filter.Ids == null),
            cancellationToken))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await InvokeHandlerAsync(filter, pagination, cancellationToken);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Result);
    }

    private async Task<ApiGetResponse<GetProductsResult>> InvokeHandlerAsync(
        GetProductsFilter filter,
        PaginationRequest pagination,
        CancellationToken cancellationToken)
    {
        return await (Task<ApiGetResponse<GetProductsResult>>)_handlerMethod.Invoke(
            _endpoint,
            new object[]
            {
                _mockSender.Object,
                filter,
                pagination,
                cancellationToken
            })!;
    }
}
