using BuildingBlocks.Extensions;
using Catalog.Application.Features.Product.Commands;
using Catalog.Domain.Entities;
using Catalog.Domain.Enums;
using Catalog.Domain.Events;
using Common.Constants;
using Common.ValueObjects;
using Marten;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace TestProject1;

[TestClass]
public sealed class UnpublishProductCommandHandlerTests
{
    private Mock<IDocumentSession> _mockSession = null!;
    private Mock<IMediator> _mockMediator = null!;
    private UnpublishProductCommandHandler _handler = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockSession = new Mock<IDocumentSession>();
        _mockMediator = new Mock<IMediator>();
        _handler = new UnpublishProductCommandHandler(_mockSession.Object, _mockMediator.Object);
    }

    [TestMethod]
    public async Task Handle_WithValidCommand_ShouldUnpublishProductAndReturnProductId()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var actor = Actor.User("test@example.com");
        var command = new UnpublishProductCommand(productId, actor);

        var productEntity = new ProductEntity
        {
            Id = productId,
            Name = "Test Product",
            Sku = "TEST-SKU-001",
            Slug = "test-product",
            Price = 99.99m,
            SalePrice = 79.99m,
            Status = ProductStatus.InStock,
            Published = true, // Initially published
            CreatedOnUtc = DateTimeOffset.UtcNow,
            CreatedBy = "admin@example.com",
            LastModifiedOnUtc = DateTimeOffset.UtcNow,
            LastModifiedBy = "admin@example.com"
        };

        _mockSession.Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        _mockSession.Setup(x => x.LoadAsync<ProductEntity>(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(productEntity);

        _mockSession.Setup(x => x.Store(It.IsAny<ProductEntity>()));

        _mockSession.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockMediator.Setup(x => x.Publish(It.IsAny<DeletedUnPublishedProductDomainEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.AreEqual(productId, result);
        Assert.IsFalse(productEntity.Published);
        _mockSession.Verify(x => x.LoadAsync<ProductEntity>(productId, It.IsAny<CancellationToken>()), Times.Once);
        _mockSession.Verify(x => x.Store(productEntity), Times.Once);
        _mockSession.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockMediator.Verify(x => x.Publish(It.IsAny<DeletedUnPublishedProductDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task Handle_WithNonExistentProduct_ShouldThrowClientValidationException()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var actor = Actor.User("test@example.com");
        var command = new UnpublishProductCommand(productId, actor);

        _mockSession.Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        _mockSession.Setup(x => x.LoadAsync<ProductEntity>(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductEntity?)null);

        // Act & Assert
        try
        {
            await _handler.Handle(command, CancellationToken.None);
            Assert.Fail("Expected ClientValidationException was not thrown.");
        }
        catch (ClientValidationException)
        {
            // Expected exception was thrown
        }

        _mockSession.Verify(x => x.Store(It.IsAny<ProductEntity>()), Times.Never);
        _mockSession.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockMediator.Verify(x => x.Publish(It.IsAny<DeletedUnPublishedProductDomainEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestMethod]
    public async Task Handle_ShouldPublishCorrectDomainEvent()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var actor = Actor.User("test@example.com");
        var command = new UnpublishProductCommand(productId, actor);

        var productEntity = new ProductEntity
        {
            Id = productId,
            Name = "Test Product",
            Sku = "TEST-SKU-001",
            Slug = "test-product",
            Price = 99.99m,
            Status = ProductStatus.InStock,
            Published = true,
            CreatedOnUtc = DateTimeOffset.UtcNow,
            CreatedBy = "admin@example.com",
            LastModifiedOnUtc = DateTimeOffset.UtcNow,
            LastModifiedBy = "admin@example.com"
        };

        _mockSession.Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        _mockSession.Setup(x => x.LoadAsync<ProductEntity>(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(productEntity);

        _mockSession.Setup(x => x.Store(It.IsAny<ProductEntity>()));
        _mockSession.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        DeletedUnPublishedProductDomainEvent? publishedEvent = null;
        _mockMediator.Setup(x => x.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Callback<object, CancellationToken>((e, ct) =>
            {
                if (e is DeletedUnPublishedProductDomainEvent domainEvent)
                    publishedEvent = domainEvent;
            })
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.IsNotNull(publishedEvent);
        Assert.AreEqual(productId, publishedEvent.ProductId);
    }
}
