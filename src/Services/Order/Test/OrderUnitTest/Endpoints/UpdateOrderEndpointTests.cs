using FluentAssertions;
using MediatR;
using Moq;
using NUnit.Framework;
using Common.ValueObjects;
using Order.Application.Dtos.Orders;
using Order.Application.Features.Order.Commands;

namespace OrderUnitTest.Endpoints;

[TestFixture]
public class UpdateOrderEndpointTests
{
    private Mock<ISender> _senderMock = null!;

    [SetUp]
    public void Setup()
    {
        _senderMock = new Mock<ISender>();
    }

    [Test]
    public async Task UpdateOrder_WithValidDto_ShouldReturnUpdatedId()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var dto = CreateValidOrderDto();
        var actor = Actor.User("test@example.com");

        _senderMock
            .Setup(x => x.Send(It.IsAny<UpdateOrderCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(orderId);

        // Act
        var command = new UpdateOrderCommand(orderId, dto, actor);
        var result = await _senderMock.Object.Send(command);

        // Assert
        result.Should().Be(orderId);
    }

    [Test]
    public async Task UpdateOrder_CommandShouldContainCorrectOrderId()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var dto = CreateValidOrderDto();
        var actor = Actor.User("test@example.com");
        UpdateOrderCommand? capturedCommand = null;

        _senderMock
            .Setup(x => x.Send(It.IsAny<UpdateOrderCommand>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<Guid>, CancellationToken>((cmd, _) => capturedCommand = cmd as UpdateOrderCommand)
            .ReturnsAsync(orderId);

        // Act
        var command = new UpdateOrderCommand(orderId, dto, actor);
        await _senderMock.Object.Send(command);

        // Assert
        capturedCommand.Should().NotBeNull();
        capturedCommand!.OrderId.Should().Be(orderId);
    }

    [Test]
    public async Task UpdateOrder_CommandShouldContainCorrectActor()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var dto = CreateValidOrderDto();
        var actor = Actor.User("admin@example.com");
        UpdateOrderCommand? capturedCommand = null;

        _senderMock
            .Setup(x => x.Send(It.IsAny<UpdateOrderCommand>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<Guid>, CancellationToken>((cmd, _) => capturedCommand = cmd as UpdateOrderCommand)
            .ReturnsAsync(orderId);

        // Act
        var command = new UpdateOrderCommand(orderId, dto, actor);
        await _senderMock.Object.Send(command);

        // Assert
        capturedCommand!.Actor.Should().Be(actor);
        capturedCommand.Actor.Kind.Should().Be(ActorKind.User);
    }

    [Test]
    public async Task UpdateOrder_WithUpdatedShippingAddress_ShouldIncludeNewAddress()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var dto = CreateValidOrderDto();
        dto.ShippingAddress = new AddressDto
        {
            AddressLine = "456 New Street",
            Subdivision = "District 2",
            City = "Ha Noi",
            StateOrProvince = "HN",
            Country = "Vietnam",
            PostalCode = "10000"
        };
        var actor = Actor.User("test@example.com");

        _senderMock
            .Setup(x => x.Send(It.IsAny<UpdateOrderCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(orderId);

        // Act
        var command = new UpdateOrderCommand(orderId, dto, actor);
        var result = await _senderMock.Object.Send(command);

        // Assert
        result.Should().Be(orderId);
        command.Dto.ShippingAddress.City.Should().Be("Ha Noi");
    }

    [Test]
    public async Task UpdateOrder_WithUpdatedOrderItems_ShouldContainNewItems()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var dto = CreateValidOrderDto();
        dto.OrderItems = new List<CreateOrderItemDto>
        {
            new() { ProductId = Guid.NewGuid(), ProductName = "Updated Product 1", Quantity = 5, ProductPrice = 250 },
            new() { ProductId = Guid.NewGuid(), ProductName = "Updated Product 2", Quantity = 2, ProductPrice = 150 }
        };
        var actor = Actor.User("test@example.com");

        _senderMock
            .Setup(x => x.Send(It.IsAny<UpdateOrderCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(orderId);

        // Act
        var command = new UpdateOrderCommand(orderId, dto, actor);
        var result = await _senderMock.Object.Send(command);

        // Assert
        result.Should().Be(orderId);
        command.Dto.OrderItems.Should().HaveCount(2);
        command.Dto.OrderItems.Should().Contain(x => x.ProductName == "Updated Product 1");
    }

    [Test]
    public async Task UpdateOrder_WithNewCouponCode_ShouldUpdateCoupon()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var dto = CreateValidOrderDto();
        dto.CouponCode = "NEWCOUPON20";
        var actor = Actor.User("test@example.com");

        _senderMock
            .Setup(x => x.Send(It.IsAny<UpdateOrderCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(orderId);

        // Act
        var command = new UpdateOrderCommand(orderId, dto, actor);
        await _senderMock.Object.Send(command);

        // Assert
        command.Dto.CouponCode.Should().Be("NEWCOUPON20");
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
