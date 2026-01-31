using Common.ValueObjects;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Payment.Application.Gateways;
using Payment.Application.Gateways.Models;
using Payment.Domain.Entities;
using Payment.Domain.Enums;
using Payment.Infrastructure.Data;
using Payment.Worker.Jobs;
using System.Reflection;

namespace PaymentUnitTest.Workers;

[TestFixture]
public class ReconcilePaymentBackgroundServiceTests
{
    private Mock<IServiceProvider> _serviceProviderMock;
    private Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
    private Mock<IServiceScope> _serviceScopeMock;
    private Mock<IPaymentGatewayFactory> _gatewayFactoryMock;
    private Mock<IPaymentGateway> _gatewayMock;
    private Mock<ILogger<ReconcilePaymentBackgroundService>> _loggerMock;
    private ApplicationDbContext _dbContext;
    private ReconcilePaymentBackgroundService _service;

    [SetUp]
    public void Setup()
    {
        _serviceProviderMock = new Mock<IServiceProvider>();
        _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        _serviceScopeMock = new Mock<IServiceScope>();
        _gatewayFactoryMock = new Mock<IPaymentGatewayFactory>();
        _gatewayMock = new Mock<IPaymentGateway>();
        _loggerMock = new Mock<ILogger<ReconcilePaymentBackgroundService>>();

        // Setup InMemory DbContext
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unique DB per test
            .Options;
        _dbContext = new ApplicationDbContext(options);

        // Setup DI Scope
        _serviceScopeMock.Setup(x => x.ServiceProvider).Returns(_serviceProviderMock.Object);
        _serviceScopeFactoryMock.Setup(x => x.CreateScope()).Returns(_serviceScopeMock.Object);
        
        _serviceProviderMock.Setup(x => x.GetService(typeof(IServiceScopeFactory)))
            .Returns(_serviceScopeFactoryMock.Object);
        
        // Mock returning dependencies from scope
        _serviceProviderMock.Setup(x => x.GetService(typeof(ApplicationDbContext)))
            .Returns(_dbContext);
        _serviceProviderMock.Setup(x => x.GetService(typeof(IPaymentGatewayFactory)))
            .Returns(_gatewayFactoryMock.Object);

        _service = new ReconcilePaymentBackgroundService(_serviceProviderMock.Object, _loggerMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _service.Dispose();
        _dbContext.Dispose();
    }

    [Test]
    public async Task ReconcileAsync_Should_Reconcile_Success_Payment()
    {
        // Arrange
        var payment = PaymentEntity.Create(Guid.NewGuid(), 100, PaymentMethod.Momo);
        // Created 20 mins ago to exceed cutoff
        var createdField = typeof(PaymentEntity).GetProperty("CreatedOnUtc");
        createdField!.SetValue(payment, DateTimeOffset.UtcNow.AddMinutes(-20));
        
        _dbContext.Payments.Add(payment);
        await _dbContext.SaveChangesAsync();

        _gatewayFactoryMock.Setup(x => x.GetGateway(PaymentMethod.Momo))
            .Returns(_gatewayMock.Object);

        _gatewayMock.Setup(x => x.VerifyPaymentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(PaymentGatewayResult.Success("VERIFIED_TRANS_123", "OK"));

        // Act
        // Use Reflection to invoke private method
        var methods = typeof(ReconcilePaymentBackgroundService)
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance);
        var reconcileMethod = methods.First(m => m.Name == "ReconcileAsync");
        
        await (Task)reconcileMethod.Invoke(_service, new object[] { CancellationToken.None })!;

        // Assert
        var updatedPayment = await _dbContext.Payments.FindAsync(payment.Id);
        updatedPayment!.Status.Should().Be(PaymentStatus.Completed);
        updatedPayment.TransactionId.Should().Be("VERIFIED_TRANS_123");
    }

    [Test]
    public async Task ReconcileAsync_Should_Mark_As_Failed_When_Gateway_Fails()
    {
        // Arrange
        var payment = PaymentEntity.Create(Guid.NewGuid(), 100, PaymentMethod.Momo);
        var createdField = typeof(PaymentEntity).GetProperty("CreatedOnUtc");
        createdField!.SetValue(payment, DateTimeOffset.UtcNow.AddMinutes(-20));
        
        _dbContext.Payments.Add(payment);
        await _dbContext.SaveChangesAsync();

        _gatewayFactoryMock.Setup(x => x.GetGateway(PaymentMethod.Momo))
            .Returns(_gatewayMock.Object);

        _gatewayMock.Setup(x => x.VerifyPaymentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(PaymentGatewayResult.Failure("NOT_FOUND", "Transaction not found"));

        // Act
        var methods = typeof(ReconcilePaymentBackgroundService)
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance);
        var reconcileMethod = methods.First(m => m.Name == "ReconcileAsync");
        
        await (Task)reconcileMethod.Invoke(_service, new object[] { CancellationToken.None })!;

        // Assert
        var updatedPayment = await _dbContext.Payments.FindAsync(payment.Id);
        updatedPayment!.Status.Should().Be(PaymentStatus.Failed);
        updatedPayment.ErrorCode.Should().Be("RECONCILE_FAILED");
    }
}
