using Microsoft.AspNetCore.Authorization;
using Common.Models.Reponses;

namespace PaymentUnitTest.Endpoints;

[TestFixture]
[Category("Integration")]
public class CreatePaymentEndpointTests
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
    public async Task CreatePayment_ShouldReturnCreated_WhenValid()
    {
        // Arrange
        var dto = new CreatePaymentDto
        {
            OrderId = Guid.NewGuid(),
            Amount = 100m,
            Method = PaymentMethod.VnPay
        };
        var paymentId = Guid.NewGuid();

        _mockSender
            .Setup(s => s.Send(It.IsAny<CreatePaymentCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(paymentId);

        // Act
        var response = await _client.PostAsJsonAsync("/admin/payments", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<ApiCreatedResponse<Guid>>();
        result.Should().NotBeNull();
        result!.Value.Should().Be(paymentId);
    }

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

[TestFixture]
[Category("Integration")]
public class GetPaymentByIdEndpointTests
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
    public async Task GetPaymentById_ShouldReturnOk_WhenPaymentExists()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var paymentDto = new PaymentDto
        {
            Id = paymentId,
            OrderId = Guid.NewGuid(),
            Amount = 150m,
            Status = PaymentStatus.Pending,
            Method = PaymentMethod.Momo,
            CreatedOnUtc = DateTimeOffset.UtcNow
        };

        _mockSender
            .Setup(s => s.Send(It.Is<GetPaymentByIdQuery>(q => q.PaymentId == paymentId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(paymentDto);

        // Act
        var response = await _client.GetAsync($"/admin/payments/{paymentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PaymentDto>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(paymentId);
        result.Amount.Should().Be(150m);
    }

    [Test]
    public async Task GetPaymentById_ShouldReturnNotFound_WhenPaymentDoesNotExist()
    {
        // Arrange
        var paymentId = Guid.NewGuid();

        _mockSender
            .Setup(s => s.Send(It.IsAny<GetPaymentByIdQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BuildingBlocks.Extensions.NotFoundException(Common.Constants.MessageCode.NotFound, paymentId));

        // Act
        var response = await _client.GetAsync($"/admin/payments/{paymentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

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

[TestFixture]
[Category("Integration")]
public class GetPaymentsEndpointTests
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
    public async Task GetPayments_ShouldReturnOk_WithPaymentsList()
    {
        // Arrange
        var payments = new List<PaymentDto>
        {
            new() { Id = Guid.NewGuid(), Amount = 100m, Status = PaymentStatus.Pending },
            new() { Id = Guid.NewGuid(), Amount = 200m, Status = PaymentStatus.Completed }
        };

        _mockSender
            .Setup(s => s.Send(It.IsAny<GetPaymentsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(payments);

        // Act
        var response = await _client.GetAsync("/admin/payments");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<List<PaymentDto>>();
        result.Should().HaveCount(2);
    }

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
