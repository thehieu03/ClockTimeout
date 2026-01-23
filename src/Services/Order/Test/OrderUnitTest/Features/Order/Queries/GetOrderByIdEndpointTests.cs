using Microsoft.AspNetCore.Authorization;

namespace OrderUnitTest.Features.Order.Queries;

[TestFixture]
public class GetOrderByIdEndpointTests
{
    private WebApplicationFactory<Program> _factory;
    private Mock<ISender> _mockSender;
    private HttpClient _client;

    [SetUp]
    public void Setup()
    {
        _mockSender = new Mock<ISender>();
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton(_mockSender.Object);
                    // Bypass Authorization
                    services.AddSingleton<IAuthorizationHandler, AllowAnonymousHandler>();
                });
            });
        _client = _factory.CreateClient();
    }

    [TearDown]
    public void TearDown()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Test]
    public async Task GetOrderById_ShouldReturnSuccess_WhenOrderExists()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var orderDto = new OrderDto 
        { 
            Id = orderId, 
            OrderNo = "ORD-123",
            Customer = new CustomerDto { Name = "Test Customer" }
        };

        _mockSender.Setup(s => s.Send(It.Is<GetOrderByIdQuery>(q => q.OrderId == orderId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(orderDto);

        // Act
        var response = await _client.GetAsync($"/admin/orders/{orderId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<OrderDto>();
        result.Should().NotBeNull();
        result.Id.Should().Be(orderId);
        result.OrderNo.Should().Be("ORD-123");
    }

    [Test]
    public async Task GetOrderById_ShouldReturnNotFound_WhenOrderDoesNotExist()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        
        _mockSender.Setup(s => s.Send(It.IsAny<GetOrderByIdQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BuildingBlocks.Extensions.NotFoundException(Common.Constants.MessageCode.OrderNotFound, orderId));

        // Act
        var response = await _client.GetAsync($"/admin/orders/{orderId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // Helper to bypass authorization in tests
    private class AllowAnonymousHandler : IAuthorizationHandler
    {
        public Task HandleAsync(AuthorizationHandlerContext context)
        {
            foreach (var requirement in context.PendingRequirements.ToList())
            {
                context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }
    }
}
