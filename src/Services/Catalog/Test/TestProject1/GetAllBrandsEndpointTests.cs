using Catalog.Api.Endpoints;
using Catalog.Application.Dtos.Brands;
using Catalog.Application.Features.Brand.Queries;
using Catalog.Application.Models.Results;
using Common.Models.Reponses;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Reflection;

namespace TestProject1;

[TestClass]
public sealed class GetAllBrandsEndpointTests
{
    private Mock<ISender> _mockSender = null!;
    private GetAllBrands _endpoint = null!;
    private MethodInfo _handlerMethod = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockSender = new Mock<ISender>();
        _endpoint = new GetAllBrands();

        // Get private handler method using reflection
        _handlerMethod = typeof(GetAllBrands).GetMethod(
            "HandleGetAllBrandsAsync",
            BindingFlags.NonPublic | BindingFlags.Instance)!;
    }

    [TestMethod]
    public async Task HandleGetAllBrandsAsync_WithValidQuery_ShouldReturnApiGetResponse()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        var brandDto1 = new BrandDto
        {
            Id = Guid.NewGuid(),
            Name = "Nike",
            Slug = "nike"
        };

        var brandDto2 = new BrandDto
        {
            Id = Guid.NewGuid(),
            Name = "Adidas",
            Slug = "adidas"
        };

        var items = new List<BrandDto> { brandDto1, brandDto2 };
        var expectedResult = new GetAllBrandsResult(items);

        _mockSender.Setup(x => x.Send(
            It.IsAny<GetAllBrandsQuery>(),
            cancellationToken))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await InvokeHandlerAsync(cancellationToken);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Result);
        Assert.IsNotNull(result.Result.Items);
        Assert.AreEqual(2, result.Result.Items.Count);
        Assert.AreEqual("Nike", result.Result.Items[0].Name);
        Assert.AreEqual("Adidas", result.Result.Items[1].Name);

        _mockSender.Verify(x => x.Send(
            It.IsAny<GetAllBrandsQuery>(),
            cancellationToken), Times.Once);
    }

    [TestMethod]
    public async Task HandleGetAllBrandsAsync_WithEmptyResult_ShouldReturnEmptyList()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;
        var expectedResult = new GetAllBrandsResult(new List<BrandDto>());

        _mockSender.Setup(x => x.Send(
            It.IsAny<GetAllBrandsQuery>(),
            cancellationToken))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await InvokeHandlerAsync(cancellationToken);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Result);
        Assert.IsNotNull(result.Result.Items);
        Assert.AreEqual(0, result.Result.Items.Count);
    }

    [TestMethod]
    public async Task HandleGetAllBrandsAsync_ShouldPassCancellationToken()
    {
        // Arrange
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        var expectedResult = new GetAllBrandsResult(new List<BrandDto>());

        _mockSender.Setup(x => x.Send(
            It.IsAny<GetAllBrandsQuery>(),
            cancellationToken))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await InvokeHandlerAsync(cancellationToken);

        // Assert
        Assert.IsNotNull(result);
        _mockSender.Verify(x => x.Send(
            It.IsAny<GetAllBrandsQuery>(),
            cancellationToken), Times.Once);
    }

    private async Task<ApiGetResponse<GetAllBrandsResult>> InvokeHandlerAsync(
        CancellationToken cancellationToken)
    {
        return await (Task<ApiGetResponse<GetAllBrandsResult>>)_handlerMethod.Invoke(
            _endpoint,
            new object[]
            {
                _mockSender.Object,
                cancellationToken
            })!;
    }
}
