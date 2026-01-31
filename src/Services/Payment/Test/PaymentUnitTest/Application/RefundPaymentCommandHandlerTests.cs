using BuildingBlocks.Extensions;
using Common.ValueObjects;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Payment.Application.Features.Payment.Commands;
using Payment.Application.Gateways;
using Payment.Application.Gateways.Models;
using Payment.Domain.Enums;
using Payment.Domain.Repositories;
using Payment.Domain.Entities;

namespace PaymentUnitTest.Application;

[TestFixture]
public class RefundPaymentCommandHandlerTests
{
    private Mock<IPaymentRepository> _paymentRepositoryMock;
    private Mock<IPaymentGatewayFactory> _gatewayFactoryMock;
    private Mock<IPaymentGateway> _gatewayMock;
    private Mock<IUnitOfWork> _unitOfWorkMock;
    private RefundPaymentCommandHandler _handler;

    [SetUp]
    public void Setup()
    {
        _paymentRepositoryMock = new Mock<IPaymentRepository>();
        _gatewayFactoryMock = new Mock<IPaymentGatewayFactory>();
        _gatewayMock = new Mock<IPaymentGateway>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new RefundPaymentCommandHandler(
            _paymentRepositoryMock.Object,
            _gatewayFactoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Test]
    public async Task Handle_Should_Refund_Successfully_When_Payment_Is_Completed()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var command = new RefundPaymentCommand(paymentId, "Defective Product", Actor.System("Tester"));
        
        // Setup Payment Entity (using Factory or constructor if available, or setting properties via reflection/helper)
        // Since PaymentEntity setters are private, we use the Create factory and then Complete it.
        var payment = PaymentEntity.Create(orderId, 100, PaymentMethod.Momo);
        payment.MarkAsProcessing("Tester");
        payment.SetTransactionId("TRANS_123", "Tester");
        payment.Complete("TRANS_123", "OK", "Tester");
        
        // Mock Repository
        _paymentRepositoryMock.Setup(x => x.GetByIdAsync(paymentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        // Mock Gateway
        _gatewayFactoryMock.Setup(x => x.GetGateway(PaymentMethod.Momo))
            .Returns(_gatewayMock.Object);

        _gatewayMock.Setup(x => x.RefundPaymentAsync(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(PaymentGatewayResult.Success("REFUND_TRANS_456", "OK"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be("Refund Successful");
        
        payment.Status.Should().Be(PaymentStatus.Refunded);
        payment.RefundReason.Should().Be("Defective Product");
        payment.RefundTransactionId.Should().Be("REFUND_TRANS_456");

        _paymentRepositoryMock.Verify(x => x.Update(payment), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_Should_Fail_When_Payment_Not_Found()
    {
        // Arrange
        var command = new RefundPaymentCommand(Guid.NewGuid(), "Reason", Actor.System("Tester"));
        
        _paymentRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PaymentEntity?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task Handle_Should_Return_Failure_When_Payment_Not_Completed()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var command = new RefundPaymentCommand(paymentId, "Reason", Actor.System("Tester"));
        
        var payment = PaymentEntity.Create(Guid.NewGuid(), 100, PaymentMethod.Momo); 
        // Status is Pending by default
        
        _paymentRepositoryMock.Setup(x => x.GetByIdAsync(paymentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain($"Cannot refund payment in status {PaymentStatus.Pending}");
        
        _gatewayFactoryMock.Verify(x => x.GetGateway(It.IsAny<PaymentMethod>()), Times.Never);
    }

    [Test]
    public async Task Handle_Should_Return_Failure_When_Gateway_Refund_Fails()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var command = new RefundPaymentCommand(paymentId, "Reason", Actor.System("Tester"));
        
        var payment = PaymentEntity.Create(Guid.NewGuid(), 100, PaymentMethod.Momo);
        payment.MarkAsProcessing("Tester");
        payment.SetTransactionId("TRANS_123", "Tester");
        payment.Complete("TRANS_123", "OK", "Tester");
        
        _paymentRepositoryMock.Setup(x => x.GetByIdAsync(paymentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        _gatewayFactoryMock.Setup(x => x.GetGateway(PaymentMethod.Momo))
            .Returns(_gatewayMock.Object);

        _gatewayMock.Setup(x => x.RefundPaymentAsync(It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(PaymentGatewayResult.Failure("FAIL", "Balance insufficient"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("Balance insufficient");
        
        payment.Status.Should().Be(PaymentStatus.Completed); // Should remain completed
    }
}
