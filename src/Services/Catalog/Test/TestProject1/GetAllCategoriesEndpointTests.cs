using Catalog.Api.Endpoints;
using Catalog.Application.Dtos.Categories;
using Catalog.Application.Features.Category.Queries;
using Catalog.Application.Models.Filters;
using Catalog.Application.Models.Results;
using Common.Models;
using Common.Models.Reponses;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Reflection;

namespace TestProject1;

[TestClass]
public sealed class GetAllCategoriesEndpointTests
{
    private Mock<ISender> _mockSender = null!;
    private GetAllCategories _endpoint = null!;
    private MethodInfo _handlerMethod = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockSender = new Mock<ISender>();
        _endpoint = new GetAllCategories();

        // Get private handler method using reflection
        _handlerMethod = typeof(GetAllCategories).GetMethod(
            "HandleGetAllCategoriesAsync",
            BindingFlags.NonPublic | BindingFlags.Instance)!;
    }

    [TestMethod]
    public async Task HandleGetAllCategoriesAsync_WithValidFilter_ShouldReturnApiGetResponse()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var filter = new GetAllCategoriesFilter(SearchText: null, ParentId: null);
        var pagination = new PaginationRequest(PageNumber: 1, PageSize: 10);

        var categoryDto1 = new CategoryDto
        {
            Id = Guid.NewGuid(),
            Name = "Electronics",
            Description = "Electronic devices",
            Slug = "electronics",
            ParentId = null,
            ParentName = null
        };

        var categoryDto2 = new CategoryDto
        {
            Id = Guid.NewGuid(),
            Name = "Fashion",
            Description = "Fashion items",
            Slug = "fashion",
            ParentId = null,
            ParentName = null
        };

        var items = new List<CategoryDto> { categoryDto1, categoryDto2 };
        var expectedResult = new GetAllCategoriesResult(items);

        _mockSender.Setup(x => x.Send(
            It.Is<GetAllCategoriesQuery>(q =>
                q.filter.SearchText == filter.SearchText &&
                q.filter.ParentId == filter.ParentId &&
                q.pagination.PageNumber == pagination.PageNumber &&
                q.pagination.PageSize == pagination.PageSize),
            cancellationToken))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await InvokeHandlerAsync(filter, pagination, cancellationToken);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Result);
        Assert.IsNotNull(result.Result.Items);
        Assert.AreEqual(2, result.Result.Items.Count);
        Assert.AreEqual("Electronics", result.Result.Items[0].Name);
        Assert.AreEqual("Fashion", result.Result.Items[1].Name);

        _mockSender.Verify(x => x.Send(
            It.Is<GetAllCategoriesQuery>(q =>
                q.filter.SearchText == filter.SearchText &&
                q.filter.ParentId == filter.ParentId),
            cancellationToken), Times.Once);
    }

    [TestMethod]
    public async Task HandleGetAllCategoriesAsync_WithSearchText_ShouldFilterCategories()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var filter = new GetAllCategoriesFilter(SearchText: "Electronics", ParentId: null);
        var pagination = new PaginationRequest(PageNumber: 1, PageSize: 10);

        var categoryDto = new CategoryDto
        {
            Id = Guid.NewGuid(),
            Name = "Electronics",
            Description = "Electronic devices",
            Slug = "electronics",
            ParentId = null,
            ParentName = null
        };

        var items = new List<CategoryDto> { categoryDto };
        var expectedResult = new GetAllCategoriesResult(items);

        _mockSender.Setup(x => x.Send(
            It.Is<GetAllCategoriesQuery>(q => q.filter.SearchText == "Electronics"),
            cancellationToken))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await InvokeHandlerAsync(filter, pagination, cancellationToken);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Result);
        Assert.AreEqual(1, result.Result.Items.Count);
        Assert.AreEqual("Electronics", result.Result.Items[0].Name);
    }

    [TestMethod]
    public async Task HandleGetAllCategoriesAsync_WithParentId_ShouldFilterByParent()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var parentId = Guid.NewGuid();
        var filter = new GetAllCategoriesFilter(SearchText: null, ParentId: parentId);
        var pagination = new PaginationRequest(PageNumber: 1, PageSize: 10);

        var categoryDto = new CategoryDto
        {
            Id = Guid.NewGuid(),
            Name = "Smartphones",
            Description = "Mobile phones",
            Slug = "smartphones",
            ParentId = parentId,
            ParentName = "Electronics"
        };

        var items = new List<CategoryDto> { categoryDto };
        var expectedResult = new GetAllCategoriesResult(items);

        _mockSender.Setup(x => x.Send(
            It.Is<GetAllCategoriesQuery>(q => q.filter.ParentId == parentId),
            cancellationToken))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await InvokeHandlerAsync(filter, pagination, cancellationToken);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Result);
        Assert.AreEqual(1, result.Result.Items.Count);
        Assert.AreEqual(parentId, result.Result.Items[0].ParentId);
        Assert.AreEqual("Electronics", result.Result.Items[0].ParentName);
    }

    [TestMethod]
    public async Task HandleGetAllCategoriesAsync_ShouldPassCancellationToken()
    {
        // Arrange
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var filter = new GetAllCategoriesFilter(SearchText: null, ParentId: null);
        var pagination = new PaginationRequest(PageNumber: 1, PageSize: 10);

        var expectedResult = new GetAllCategoriesResult(new List<CategoryDto>());

        _mockSender.Setup(x => x.Send(
            It.IsAny<GetAllCategoriesQuery>(),
            cancellationToken))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await InvokeHandlerAsync(filter, pagination, cancellationToken);

        // Assert
        Assert.IsNotNull(result);
        _mockSender.Verify(x => x.Send(
            It.IsAny<GetAllCategoriesQuery>(),
            cancellationToken), Times.Once);
    }

    private async Task<ApiGetResponse<GetAllCategoriesResult>> InvokeHandlerAsync(
        GetAllCategoriesFilter filter,
        PaginationRequest pagination,
        CancellationToken cancellationToken)
    {
        return await (Task<ApiGetResponse<GetAllCategoriesResult>>)_handlerMethod.Invoke(
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
