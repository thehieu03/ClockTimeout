using Catalog.Api.Endpoints;
using Catalog.Application.Features.Product.Commands;
using Common.Models.Reponses;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Reflection;

namespace TestProject1;

[TestClass]
public sealed class DeleteProductEndpointTests
{
    private Mock<ISender> _mockSender = null!;
    private DeleteProduct _endpoint = null!;
    private MethodInfo _handlerMethod = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockSender = new Mock<ISender>();
        _endpoint = new DeleteProduct();

        // Get private handler method using reflection
        _handlerMethod = typeof(DeleteProduct).GetMethod(
            "HandleDeleteProductAsync",
            BindingFlags.NonPublic | BindingFlags.Instance)!;
    }

    [TestMethod]
    public async Task HandleDeleteProductAsync_WithValidProductId_ShouldReturnApiDeletedResponse()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;

        _mockSender.Setup(x => x.Send(
            It.Is<DeleteProductCommand>(cmd => cmd.ProductId == productId),
            cancellationToken))
            .ReturnsAsync(Unit.Value);

        // Act
        var result = await InvokeHandlerAsync(productId, cancellationToken);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(productId, result.Value);
        _mockSender.Verify(x => x.Send(
            It.Is<DeleteProductCommand>(cmd => cmd.ProductId == productId),
            cancellationToken), Times.Once);
    }

    [TestMethod]
    public async Task HandleDeleteProductAsync_ShouldPassCancellationToken()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        _mockSender.Setup(x => x.Send(
            It.IsAny<DeleteProductCommand>(),
            cancellationToken))
            .ReturnsAsync(Unit.Value);

        // Act
        var result = await InvokeHandlerAsync(productId, cancellationToken);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(productId, result.Value);
        _mockSender.Verify(x => x.Send(
            It.Is<DeleteProductCommand>(cmd => cmd.ProductId == productId),
            cancellationToken), Times.Once);
    }

    private async Task<ApiDeletedResponse<Guid>> InvokeHandlerAsync(
        Guid productId,
        CancellationToken cancellationToken)
    {
        return await (Task<ApiDeletedResponse<Guid>>)_handlerMethod.Invoke(
            _endpoint,
            new object[]
            {
                _mockSender.Object,
                productId,
                cancellationToken
            })!;
    }
}
