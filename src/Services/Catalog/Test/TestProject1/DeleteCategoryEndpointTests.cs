using Catalog.Api.Endpoints;
using Catalog.Application.Features.Category.Commands;
using Common.Models.Reponses;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Reflection;

namespace TestProject1;

[TestClass]
public sealed class DeleteCategoryEndpointTests
{
    private Mock<ISender> _mockSender = null!;
    private DeleteCategory _endpoint = null!;
    private MethodInfo _handlerMethod = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockSender = new Mock<ISender>();
        _endpoint = new DeleteCategory();

        // Get private handler method using reflection
        _handlerMethod = typeof(DeleteCategory).GetMethod(
            "HandleDeleteCategoryAsync",
            BindingFlags.NonPublic | BindingFlags.Instance)!;
    }

    [TestMethod]
    public async Task HandleDeleteCategoryAsync_WithValidCategoryId_ShouldReturnApiDeletedResponse()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;

        _mockSender.Setup(x => x.Send(
            It.Is<DeleteCategoryCommand>(cmd => cmd.CategoryId == categoryId),
            cancellationToken))
            .ReturnsAsync(Unit.Value);

        // Act
        var result = await InvokeHandlerAsync(categoryId, cancellationToken);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(categoryId, result.Value);
        _mockSender.Verify(x => x.Send(
            It.Is<DeleteCategoryCommand>(cmd => cmd.CategoryId == categoryId),
            cancellationToken), Times.Once);
    }

    [TestMethod]
    public async Task HandleDeleteCategoryAsync_ShouldPassCancellationToken()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        _mockSender.Setup(x => x.Send(
            It.IsAny<DeleteCategoryCommand>(),
            cancellationToken))
            .ReturnsAsync(Unit.Value);

        // Act
        var result = await InvokeHandlerAsync(categoryId, cancellationToken);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(categoryId, result.Value);
        _mockSender.Verify(x => x.Send(
            It.Is<DeleteCategoryCommand>(cmd => cmd.CategoryId == categoryId),
            cancellationToken), Times.Once);
    }

    private async Task<ApiDeletedResponse<Guid>> InvokeHandlerAsync(
        Guid categoryId,
        CancellationToken cancellationToken)
    {
        return await (Task<ApiDeletedResponse<Guid>>)_handlerMethod.Invoke(
            _endpoint,
            new object[]
            {
                _mockSender.Object,
                categoryId,
                cancellationToken
            })!;
    }
}
