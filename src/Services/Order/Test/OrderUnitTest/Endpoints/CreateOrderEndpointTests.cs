using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using Common.ValueObjects;
using Order.Application.Dtos.Orders;
using Order.Application.Features.Order.Commands;

namespace OrderUnitTest.Endpoints;

[TestFixture]
public class CreateOrderEndpointTests
{
    private Mock<ISender> _senderMock = null!;

    [SetUp]
    public void Setup()
    {
        _senderMock = new Mock<ISender>();
    }

    [Test]
    public async Task CreateOrder_WithValidDto_ShouldReturnCreatedResponse()
    {
        // Arrange
        var dto = CreateValidOrderDto();
        var expectedOrderId = Guid.NewGuid();

        _senderMock
            .Setup(x => x.Send(It.IsAny<CreateOrderCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedOrderId);

        // Act
        var command = new CreateOrderCommand(dto, Actor.User("test@example.com"));
        var result = await _senderMock.Object.Send(command);

        // Assert
        result.Should().Be(expectedOrderId);
        _senderMock.Verify(x => x.Send(It.IsAny<CreateOrderCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task CreateOrder_WithEmptyOrderItems_ShouldStillSendCommand()
    {
        // Arrange
        var dto = CreateValidOrderDto();
        dto.OrderItems = new List<CreateOrderItemDto>();
        var expectedOrderId = Guid.NewGuid();

        _senderMock
            .Setup(x => x.Send(It.IsAny<CreateOrderCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedOrderId);

        // Act
        var command = new CreateOrderCommand(dto, Actor.User("test@example.com"));
        var result = await _senderMock.Object.Send(command);

        // Assert
        result.Should().Be(expectedOrderId);
    }

    [Test]
    public async Task CreateOrder_WithMultipleOrderItems_ShouldSendCommandWithAllItems()
    {
        // Arrange
        var dto = CreateValidOrderDto();
        dto.OrderItems = new List<CreateOrderItemDto>
        {
            new() { ProductId = Guid.NewGuid(), ProductName = "Product 1", Quantity = 2, ProductPrice = 100 },
            new() { ProductId = Guid.NewGuid(), ProductName = "Product 2", Quantity = 1, ProductPrice = 200 },
            new() { ProductId = Guid.NewGuid(), ProductName = "Product 3", Quantity = 3, ProductPrice = 50 }
        };
        var expectedOrderId = Guid.NewGuid();

        _senderMock
            .Setup(x => x.Send(It.IsAny<CreateOrderCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedOrderId);

        // Act
        var command = new CreateOrderCommand(dto, Actor.User("test@example.com"));
        var result = await _senderMock.Object.Send(command);

        // Assert
        result.Should().Be(expectedOrderId);
        command.Dto.OrderItems.Should().HaveCount(3);
    }

    [Test]
    public async Task CreateOrder_WithCouponCode_ShouldIncludeCouponInCommand()
    {
        // Arrange
        var dto = CreateValidOrderDto();
        dto.CouponCode = "DISCOUNT10";
        var expectedOrderId = Guid.NewGuid();

        _senderMock
            .Setup(x => x.Send(It.IsAny<CreateOrderCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedOrderId);

        // Act
        var command = new CreateOrderCommand(dto, Actor.User("test@example.com"));
        var result = await _senderMock.Object.Send(command);

        // Assert
        result.Should().Be(expectedOrderId);
        command.Dto.CouponCode.Should().Be("DISCOUNT10");
    }

    [Test]
    public async Task CreateOrder_CommandShouldContainCorrectActor()
    {
        // Arrange
        var dto = CreateValidOrderDto();
        var actor = Actor.User("user@test.com");
        CreateOrderCommand? capturedCommand = null;

        _senderMock
            .Setup(x => x.Send(It.IsAny<CreateOrderCommand>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<Guid>, CancellationToken>((cmd, _) => capturedCommand = cmd as CreateOrderCommand)
            .ReturnsAsync(Guid.NewGuid());

        // Act
        var command = new CreateOrderCommand(dto, actor);
        await _senderMock.Object.Send(command);

        // Assert
        capturedCommand.Should().NotBeNull();
        capturedCommand!.Actor.Should().Be(actor);
        capturedCommand.Actor.Kind.Should().Be(ActorKind.User);
    }

    private static CreateOrUpdateOrderDto CreateValidOrderDto()
    {
        return new CreateOrUpdateOrderDto
        {
            BasketId = Guid.NewGuid(),
            Customer = new CustomerDto
            {
                Name = "John Doe",
                Email = "john@example.com",
                PhoneNumber = "0123456789"
            },
            ShippingAddress = new AddressDto
            {
                AddressLine = "123 Main St",
                Subdivision = "District 1",
                City = "Ho Chi Minh",
                StateOrProvince = "HCM",
                Country = "Vietnam",
                PostalCode = "70000"
            },
            OrderItems = new List<CreateOrderItemDto>
            {
                new()
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Test Product",
                    Quantity = 1,
                    ProductPrice = 100
                }
            },
            CouponCode = "",
            Notes = "Test order"
        };
    }
}
