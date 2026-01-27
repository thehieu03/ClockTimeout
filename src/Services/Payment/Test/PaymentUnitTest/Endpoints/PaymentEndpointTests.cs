using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory;
using Microsoft.Extensions.Configuration;
using Common.Models.Reponses;
using Payment.Infrastructure.Data;

namespace PaymentUnitTest.Endpoints;

[TestFixture]
[Category("Integration")]
public class CreatePaymentEndpointTests
{
    private WebApplicationFactory<Program> _factory = null!;
    private Mock<ISender> _mockSender = null!;
    private HttpClient _client = null!;

    [SetUp]
    public void Setup()
    {
        _mockSender = new Mock<ISender>();
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, config) =>
                {
                    // Override connection string to use in-memory database for tests
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        {"ConnectionStrings:DbType", "InMemory"},
                        {"ConnectionStrings:Database", "TestDb"}
                    });
                });
                
                builder.ConfigureTestServices(services =>
                {
                    // Remove the real DbContext registration
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                    if (descriptor != null)
                        services.Remove(descriptor);
                    
                    // Add in-memory database
                    services.AddDbContext<ApplicationDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("TestPaymentDb_" + Guid.NewGuid());
                    });
                    
                    // Configure authorization to allow anonymous access for testing
                    services.AddAuthorization(options =>
                    {
                        options.DefaultPolicy = new AuthorizationPolicyBuilder()
                            .RequireAssertion(_ => true)
                            .Build();
                    });
                    
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
        var paymentDto = new PaymentDto
        {
            Id = paymentId,
            OrderId = dto.OrderId,
            Amount = dto.Amount,
            Method = dto.Method,
            Status = PaymentStatus.Pending
        };

        _mockSender
            .Setup(s => s.Send(It.IsAny<CreatePaymentCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(paymentDto);

        // Act
        var response = await _client.PostAsJsonAsync("/admin/payments", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<ApiCreatedResponse<PaymentDto>>();
        result.Should().NotBeNull();
        result!.Value.Id.Should().Be(paymentId);
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
    private WebApplicationFactory<Program> _factory = null!;
    private Mock<ISender> _mockSender = null!;
    private HttpClient _client = null!;

    [SetUp]
    public void Setup()
    {
        _mockSender = new Mock<ISender>();
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        {"ConnectionStrings:DbType", "InMemory"},
                        {"ConnectionStrings:Database", "TestDb"}
                    });
                });
                
                builder.ConfigureTestServices(services =>
                {
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                    if (descriptor != null)
                        services.Remove(descriptor);
                    
                    services.AddDbContext<ApplicationDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("TestPaymentDb_" + Guid.NewGuid());
                    });
                    
                    services.AddAuthorization(options =>
                    {
                        options.DefaultPolicy = new AuthorizationPolicyBuilder()
                            .RequireAssertion(_ => true)
                            .Build();
                    });
                    
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
    private WebApplicationFactory<Program> _factory = null!;
    private Mock<ISender> _mockSender = null!;
    private HttpClient _client = null!;

    [SetUp]
    public void Setup()
    {
        _mockSender = new Mock<ISender>();
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        {"ConnectionStrings:DbType", "InMemory"},
                        {"ConnectionStrings:Database", "TestDb"}
                    });
                });
                
                builder.ConfigureTestServices(services =>
                {
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                    if (descriptor != null)
                        services.Remove(descriptor);
                    
                    services.AddDbContext<ApplicationDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("TestPaymentDb_" + Guid.NewGuid());
                    });
                    
                    services.AddAuthorization(options =>
                    {
                        options.DefaultPolicy = new AuthorizationPolicyBuilder()
                            .RequireAssertion(_ => true)
                            .Build();
                    });
                    
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
