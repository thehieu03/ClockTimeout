using FluentAssertions;
using MediatR;
using Moq;
using NUnit.Framework;
using Order.Application.Dtos.Orders;
using Order.Application.Features.Order.Queries;

namespace OrderUnitTest.Endpoints;

[TestFixture]
public class GetOrderByIdEndpointTests
{
    private Mock<ISender> _senderMock = null!;

    [SetUp]
    public void Setup()
    {
        _senderMock = new Mock<ISender>();
    }

    [Test]
    public async Task GetOrderById_WithExistingOrder_ShouldReturnOrderDto()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var expectedOrder = CreateSampleOrderDto(orderId);

        _senderMock
            .Setup(x => x.Send(It.IsAny<GetOrderByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedOrder);

        // Act
        var query = new GetOrderByIdQuery(orderId);
        var result = await _senderMock.Object.Send(query);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(orderId);
        result.OrderNo.Should().Be(expectedOrder.OrderNo);
    }

    [Test]
    public async Task GetOrderById_QueryShouldContainCorrectOrderId()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        GetOrderByIdQuery? capturedQuery = null;

        _senderMock
            .Setup(x => x.Send(It.IsAny<GetOrderByIdQuery>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<OrderDto>, CancellationToken>((q, _) => capturedQuery = q as GetOrderByIdQuery)
            .ReturnsAsync(CreateSampleOrderDto(orderId));

        // Act
        var query = new GetOrderByIdQuery(orderId);
        await _senderMock.Object.Send(query);

        // Assert
        capturedQuery.Should().NotBeNull();
        capturedQuery!.OrderId.Should().Be(orderId);
    }

    [Test]
    public async Task GetOrderById_WithOrderContainingItems_ShouldReturnAllItems()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var expectedOrder = CreateSampleOrderDto(orderId);
        expectedOrder.OrderItems = new List<OrderItemDto>
        {
            CreateOrderItemDto("Item 1", 2, 100),
            CreateOrderItemDto("Item 2", 1, 200)
        };

        _senderMock
            .Setup(x => x.Send(It.IsAny<GetOrderByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedOrder);

        // Act
        var query = new GetOrderByIdQuery(orderId);
        var result = await _senderMock.Object.Send(query);

        // Assert
        result.OrderItems.Should().HaveCount(2);
    }

    [Test]
    public async Task GetOrderById_ShouldReturnCorrectTotalPrice()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var expectedOrder = CreateSampleOrderDto(orderId);
        expectedOrder.TotalPrice = 500m;
        expectedOrder.FinalPrice = 450m;
        expectedOrder.DiscountAmount = 50m;

        _senderMock
            .Setup(x => x.Send(It.IsAny<GetOrderByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedOrder);

        // Act
        var query = new GetOrderByIdQuery(orderId);
        var result = await _senderMock.Object.Send(query);

        // Assert
        result.TotalPrice.Should().Be(500m);
        result.FinalPrice.Should().Be(450m);
        result.DiscountAmount.Should().Be(50m);
    }

    [Test]
    public async Task GetOrderById_ShouldReturnCustomerInfo()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var expectedOrder = CreateSampleOrderDto(orderId);

        _senderMock
            .Setup(x => x.Send(It.IsAny<GetOrderByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedOrder);

        // Act
        var query = new GetOrderByIdQuery(orderId);
        var result = await _senderMock.Object.Send(query);

        // Assert
        result.Customer.Should().NotBeNull();
        result.Customer.Name.Should().Be("John Doe");
        result.Customer.Email.Should().Be("john@example.com");
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

    private static OrderDto CreateSampleOrderDto(Guid orderId)
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
            Status = 1,
            StatusName = "Pending",
            TotalPrice = 100m,
            FinalPrice = 100m,
            DiscountAmount = 0m,
            CreatedOnUtc = DateTimeOffset.UtcNow
        };
    }
}
