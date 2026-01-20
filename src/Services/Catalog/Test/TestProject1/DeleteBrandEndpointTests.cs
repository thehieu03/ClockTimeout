using BuildingBlocks.Extensions;
using Catalog.Api.Endpoints;
using Catalog.Application.Features.Brand.Commands;
using Common.Models.Reponses;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Reflection;

namespace TestProject1;

[TestClass]
public sealed class DeleteBrandEndpointTests
{
    private Mock<ISender> _mockSender = null!;
    private DeleteBrand _endpoint = null!;
    private MethodInfo _handlerMethod = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockSender = new Mock<ISender>();
        _endpoint = new DeleteBrand();

        // Get private handler method using reflection
        _handlerMethod = typeof(DeleteBrand).GetMethod(
            "HandleDeleteBrandAsync",
            BindingFlags.NonPublic | BindingFlags.Instance)!;
    }

    [TestMethod]
    public async Task HandleDeleteBrandAsync_WithValidBrandId_ShouldReturnApiDeletedResponse()
    {
        // Arrange
        var brandId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;

        _mockSender.Setup(x => x.Send(
            It.IsAny<DeleteBrandCommand>(),
            cancellationToken))
            .ReturnsAsync(Unit.Value);

        // Act
        var result = await InvokeHandlerAsync(brandId, cancellationToken);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(brandId, result.Value);
        _mockSender.Verify(x => x.Send(
            It.Is<DeleteBrandCommand>(cmd => cmd.BrandId == brandId),
            cancellationToken), Times.Once);
    }

    [TestMethod]
    public async Task HandleDeleteBrandAsync_ShouldPassCancellationToken()
    {
        // Arrange
        var brandId = Guid.NewGuid();
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        _mockSender.Setup(x => x.Send(
            It.IsAny<DeleteBrandCommand>(),
            cancellationToken))
            .ReturnsAsync(Unit.Value);

        // Act
        var result = await InvokeHandlerAsync(brandId, cancellationToken);

        // Assert
        Assert.IsNotNull(result);
        _mockSender.Verify(x => x.Send(
            It.IsAny<DeleteBrandCommand>(),
            cancellationToken), Times.Once);
    }

    private async Task<ApiDeletedResponse<Guid>> InvokeHandlerAsync(
        Guid brandId,
        CancellationToken cancellationToken)
    {
        return await (Task<ApiDeletedResponse<Guid>>)_handlerMethod.Invoke(
            _endpoint,
            new object[]
            {
                _mockSender.Object,
                brandId,
                cancellationToken
            })!;
    }
}
