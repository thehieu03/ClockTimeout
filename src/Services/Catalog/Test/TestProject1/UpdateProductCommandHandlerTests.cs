using BuildingBlocks.Extensions;
using Catalog.Application.Dtos.Products;
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
public sealed class UpdateProductCommandHandlerTests
{
    private Mock<IDocumentSession> _mockSession = null!;
    private Mock<IMediator> _mockMediator = null!;
    private UpdateProductCommandHandler _handler = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockSession = new Mock<IDocumentSession>();
        _mockMediator = new Mock<IMediator>();
        _handler = new UpdateProductCommandHandler(_mockSession.Object, _mockMediator.Object);
    }

    [TestMethod]
    public async Task Handle_WithValidCommand_ShouldUpdateProductAndReturnProductId()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var actor = Actor.User("test@example.com");
        var dto = new UpdateProductDto
        {
            Name = "Updated Product",
            Sku = "UPDATED-SKU-001",
            ShortDescription = "Updated short description",
            LongDescription = "Updated long description",
            Price = 149.99m,
            SalePrice = 119.99m,
            CategoryIds = new List<Guid> { Guid.NewGuid() },
            BrandId = Guid.NewGuid(),
            Colors = new List<string> { "Red", "Blue" },
            Sizes = new List<string> { "S", "M", "L" },
            Tags = new List<string> { "tag1", "tag2" },
            Published = true,
            Featured = true,
            SEOTitle = "SEO Title",
            SEODescription = "SEO Description",
            Barcode = "123456789",
            Unit = "kg",
            Weight = 1.5m
        };

        var command = new UpdateProductCommand(productId, dto, actor);

        var productEntity = new ProductEntity
        {
            Id = productId,
            Name = "Original Product",
            Sku = "ORIGINAL-SKU-001",
            Slug = "original-product",
            Price = 99.99m,
            SalePrice = 79.99m,
            Status = ProductStatus.InStock,
            Published = false,
            Featured = false,
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

        _mockMediator.Setup(x => x.Publish(It.IsAny<UpsertedProductDomainEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.AreEqual(productId, result);
        Assert.AreEqual("Updated Product", productEntity.Name);
        Assert.AreEqual("UPDATED-SKU-001", productEntity.Sku);
        Assert.AreEqual(149.99m, productEntity.Price);
        Assert.AreEqual(119.99m, productEntity.SalePrice);
        Assert.IsTrue(productEntity.Published);
        Assert.IsTrue(productEntity.Featured);
        _mockSession.Verify(x => x.LoadAsync<ProductEntity>(productId, It.IsAny<CancellationToken>()), Times.Once);
        _mockSession.Verify(x => x.Store(productEntity), Times.Once);
        _mockSession.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockMediator.Verify(x => x.Publish(It.IsAny<UpsertedProductDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task Handle_WithNonExistentProduct_ShouldThrowClientValidationException()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var actor = Actor.User("test@example.com");
        var dto = new UpdateProductDto
        {
            Name = "Updated Product",
            Sku = "UPDATED-SKU-001",
            ShortDescription = "Updated short description",
            LongDescription = "Updated long description",
            Price = 149.99m
        };

        var command = new UpdateProductCommand(productId, dto, actor);

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
        _mockMediator.Verify(x => x.Publish(It.IsAny<UpsertedProductDomainEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestMethod]
    public async Task Handle_WithPublishedStatusChange_ShouldCallPublishOrUnpublish()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var actor = Actor.User("test@example.com");
        var dto = new UpdateProductDto
        {
            Name = "Updated Product",
            Sku = "UPDATED-SKU-001",
            ShortDescription = "Updated short description",
            LongDescription = "Updated long description",
            Price = 149.99m,
            Published = true
        };

        var command = new UpdateProductCommand(productId, dto, actor);

        var productEntity = new ProductEntity
        {
            Id = productId,
            Name = "Original Product",
            Sku = "ORIGINAL-SKU-001",
            Slug = "original-product",
            Price = 99.99m,
            Status = ProductStatus.InStock,
            Published = false, // Initially unpublished
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

        _mockMediator.Setup(x => x.Publish(It.IsAny<UpsertedProductDomainEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.IsTrue(productEntity.Published);
    }

    [TestMethod]
    public async Task Handle_WithUnpublishStatusChange_ShouldCallUnpublish()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var actor = Actor.User("test@example.com");
        var dto = new UpdateProductDto
        {
            Name = "Updated Product",
            Sku = "UPDATED-SKU-001",
            ShortDescription = "Updated short description",
            LongDescription = "Updated long description",
            Price = 149.99m,
            Published = false
        };

        var command = new UpdateProductCommand(productId, dto, actor);

        var productEntity = new ProductEntity
        {
            Id = productId,
            Name = "Original Product",
            Sku = "ORIGINAL-SKU-001",
            Slug = "original-product",
            Price = 99.99m,
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

        _mockMediator.Setup(x => x.Publish(It.IsAny<UpsertedProductDomainEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.IsFalse(productEntity.Published);
    }
}
