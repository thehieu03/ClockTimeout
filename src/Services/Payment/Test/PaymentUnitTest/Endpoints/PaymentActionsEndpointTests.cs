using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory;
using Microsoft.Extensions.Configuration;
using Payment.Infrastructure.Data;

namespace PaymentUnitTest.Endpoints;

[TestFixture]
[Category("Integration")]
public class CompletePaymentEndpointTests
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
    public async Task CompletePayment_ShouldReturnNoContent_WhenSuccessful()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var dto = new CompletePaymentDto { TransactionId = "TXN-123456" };

        _mockSender
            .Setup(s => s.Send(It.Is<CompletePaymentCommand>(c =>
                c.PaymentId == paymentId && c.TransactionId == "TXN-123456"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Value);

        // Act
        var response = await _client.PostAsJsonAsync($"/admin/payments/{paymentId}/complete", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Test]
    public async Task CompletePayment_ShouldReturnNotFound_WhenPaymentDoesNotExist()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var dto = new CompletePaymentDto { TransactionId = "TXN-123" };

        _mockSender
            .Setup(s => s.Send(It.IsAny<CompletePaymentCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BuildingBlocks.Extensions.NotFoundException(Common.Constants.MessageCode.NotFound, paymentId));

        // Act
        var response = await _client.PostAsJsonAsync($"/admin/payments/{paymentId}/complete", dto);

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
public class FailPaymentEndpointTests
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
    public async Task FailPayment_ShouldReturnNoContent_WhenSuccessful()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var dto = new FailPaymentDto { ErrorMessage = "Card declined" };

        _mockSender
            .Setup(s => s.Send(It.Is<FailPaymentCommand>(c =>
                c.PaymentId == paymentId && c.ErrorMessage == "Card declined"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Value);

        // Act
        var response = await _client.PostAsJsonAsync($"/admin/payments/{paymentId}/fail", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
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
public class RefundPaymentEndpointTests
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
    public async Task RefundPayment_ShouldReturnNoContent_WhenSuccessful()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var dto = new RefundPaymentDto
        {
            RefundReason = "Customer request",
            RefundTransactionId = "REFUND-123"
        };

        _mockSender
            .Setup(s => s.Send(It.Is<RefundPaymentCommand>(c =>
                c.PaymentId == paymentId &&
                c.RefundReason == "Customer request" &&
                c.RefundTransactionId == "REFUND-123"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Unit.Value);

        // Act
        var response = await _client.PostAsJsonAsync($"/admin/payments/{paymentId}/refund", dto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Test]
    public async Task RefundPayment_ShouldReturnNotFound_WhenPaymentDoesNotExist()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var dto = new RefundPaymentDto { RefundReason = "Customer request" };

        _mockSender
            .Setup(s => s.Send(It.IsAny<RefundPaymentCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BuildingBlocks.Extensions.NotFoundException(Common.Constants.MessageCode.NotFound, paymentId));

        // Act
        var response = await _client.PostAsJsonAsync($"/admin/payments/{paymentId}/refund", dto);

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
public class GetPaymentByOrderIdEndpointTests
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
    public async Task GetPaymentByOrderId_ShouldReturnOk_WhenPaymentExists()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var paymentDto = new PaymentDto
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            Amount = 250m,
            Status = PaymentStatus.Completed,
            TransactionId = "TXN-456"
        };

        _mockSender
            .Setup(s => s.Send(It.Is<GetPaymentByOrderIdQuery>(q => q.OrderId == orderId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(paymentDto);

        // Act
        var response = await _client.GetAsync($"/payments/by-order/{orderId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PaymentDto>();
        result.Should().NotBeNull();
        result!.OrderId.Should().Be(orderId);
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
