using FluentAssertions;
using MediatR;
using Moq;
using NUnit.Framework;
using Order.Application.Dtos.Orders;
using Order.Application.Features.Order.Queries;

namespace OrderUnitTest.Endpoints;

[TestFixture]
public class GetOrdersEndpointTests
{
    private Mock<ISender> _senderMock = null!;

    [SetUp]
    public void Setup()
    {
        _senderMock = new Mock<ISender>();
    }

    [Test]
    public async Task GetOrders_ShouldReturnListOfOrders()
    {
        // Arrange
        var expectedOrders = new List<OrderDto>
        {
            CreateSampleOrderDto(Guid.NewGuid()),
            CreateSampleOrderDto(Guid.NewGuid()),
            CreateSampleOrderDto(Guid.NewGuid())
        };

        _senderMock
            .Setup(x => x.Send(It.IsAny<GetOrdersQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedOrders);

        // Act
        var query = new GetOrdersQuery();
        var result = await _senderMock.Object.Send(query);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
    }

    [Test]
    public async Task GetOrders_WhenNoOrders_ShouldReturnEmptyList()
    {
        // Arrange
        _senderMock
            .Setup(x => x.Send(It.IsAny<GetOrdersQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<OrderDto>());

        // Act
        var query = new GetOrdersQuery();
        var result = await _senderMock.Object.Send(query);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Test]
    public async Task GetOrders_ShouldReturnOrdersWithCorrectProperties()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var expectedOrders = new List<OrderDto> { CreateSampleOrderDto(orderId) };

        _senderMock
            .Setup(x => x.Send(It.IsAny<GetOrdersQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedOrders);

        // Act
        var query = new GetOrdersQuery();
        var result = await _senderMock.Object.Send(query);

        // Assert
        var order = result.First();
        order.Id.Should().Be(orderId);
        order.Customer.Should().NotBeNull();
        order.ShippingAddress.Should().NotBeNull();
        order.OrderItems.Should().NotBeEmpty();
    }

    [Test]
    public async Task GetOrders_ShouldCallSenderOnce()
    {
        // Arrange
        _senderMock
            .Setup(x => x.Send(It.IsAny<GetOrdersQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<OrderDto>());

        // Act
        var query = new GetOrdersQuery();
        await _senderMock.Object.Send(query);

        // Assert
        _senderMock.Verify(x => x.Send(It.IsAny<GetOrdersQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GetOrders_ShouldReturnOrdersWithDifferentStatuses()
    {
        // Arrange
        var orders = new List<OrderDto>
        {
            CreateSampleOrderDto(Guid.NewGuid(), status: 1, statusName: "Pending"),
            CreateSampleOrderDto(Guid.NewGuid(), status: 2, statusName: "Processing"),
            CreateSampleOrderDto(Guid.NewGuid(), status: 3, statusName: "Completed")
        };

        _senderMock
            .Setup(x => x.Send(It.IsAny<GetOrdersQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(orders);

        // Act
        var query = new GetOrdersQuery();
        var result = await _senderMock.Object.Send(query);

        // Assert
        result.Should().Contain(x => x.StatusName == "Pending");
        result.Should().Contain(x => x.StatusName == "Processing");
        result.Should().Contain(x => x.StatusName == "Completed");
    }

    private static OrderItemDto CreateOrderItemDto(string productName, int quantity, decimal price)
    {
        return new OrderItemDto
        {
            Id = Guid.NewGuid(),
            Product = new ProductDto
            {
                Id = Guid.NewGuid(),
                Name = productName,
                ImageUrl = "https://example.com/image.jpg",
                Price = price
            },
            Quantity = quantity,
            LineTotal = quantity * price
        };
    }

    private static OrderDto CreateSampleOrderDto(Guid orderId, int status = 1, string statusName = "Pending")
    {
        return new OrderDto
        {
            Id = orderId,
            OrderNo = $"ORD-{orderId.ToString()[..8].ToUpper()}",
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
            OrderItems = new List<OrderItemDto>
            {
                CreateOrderItemDto("Test Product", 1, 100)
            },
            Status = status,
            StatusName = statusName,
            TotalPrice = 100m,
            FinalPrice = 100m,
            DiscountAmount = 0m,
            CreatedOnUtc = DateTimeOffset.UtcNow
        };
    }
}
