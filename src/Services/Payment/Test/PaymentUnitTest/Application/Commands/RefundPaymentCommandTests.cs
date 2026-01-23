using BuildingBlocks.Extensions;

namespace PaymentUnitTest.Application.Commands;

[TestFixture]
[Category("Unit")]
public class RefundPaymentCommandTests
{
    private Mock<IUnitOfWork> _mockUnitOfWork;
    private Mock<IPaymentRepository> _mockPaymentRepository;
    private RefundPaymentCommandHandler _handler;

    [SetUp]
    public void Setup()
    {
        _mockPaymentRepository = new Mock<IPaymentRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockUnitOfWork.Setup(u => u.Payments).Returns(_mockPaymentRepository.Object);

        _handler = new RefundPaymentCommandHandler(_mockUnitOfWork.Object);
    }

    [Test]
    public async Task Handle_ShouldRefundCompletedPayment()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var refundReason = "Customer requested refund";
        var refundTransactionId = "REFUND-123";
        var payment = PaymentEntity.Create(Guid.NewGuid(), 100m, PaymentMethod.Momo);
        payment.Complete("TXN-123");

        _mockPaymentRepository
            .Setup(r => r.GetByIdAsync(paymentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = new RefundPaymentCommand(paymentId, refundReason, refundTransactionId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        payment.Status.Should().Be(PaymentStatus.Refunded);
        payment.RefundReason.Should().Be(refundReason);
        payment.RefundTransactionId.Should().Be(refundTransactionId);
        _mockPaymentRepository.Verify(r => r.Update(payment), Times.Once);
    }

    [Test]
    public async Task Handle_WhenPaymentNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        _mockPaymentRepository
            .Setup(r => r.GetByIdAsync(paymentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PaymentEntity?)null);

        var command = new RefundPaymentCommand(paymentId, "Refund reason");

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task Handle_WhenPaymentNotCompleted_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var payment = PaymentEntity.Create(Guid.NewGuid(), 100m, PaymentMethod.VnPay);
        // Payment is still pending, not completed

        _mockPaymentRepository
            .Setup(r => r.GetByIdAsync(paymentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        var command = new RefundPaymentCommand(paymentId, "Refund reason");

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}

[TestFixture]
[Category("Unit")]
public class RefundPaymentCommandValidatorTests
{
    private RefundPaymentCommandValidator _validator;

    [SetUp]
    public void Setup()
    {
        _validator = new RefundPaymentCommandValidator();
    }

    [Test]
    public void Validate_WithValidCommand_ShouldPass()
    {
        // Arrange
        var command = new RefundPaymentCommand(Guid.NewGuid(), "Customer request");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Test]
    public void Validate_WithEmptyPaymentId_ShouldFail()
    {
        // Arrange
        var command = new RefundPaymentCommand(Guid.Empty, "Reason");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Test]
    public void Validate_WithNullRefundReason_ShouldPass()
    {
        // Arrange - RefundReason is optional
        var command = new RefundPaymentCommand(Guid.NewGuid(), null);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Test]
    public void Validate_WithTooLongRefundReason_ShouldFail()
    {
        // Arrange
        var longReason = new string('x', 501);
        var command = new RefundPaymentCommand(Guid.NewGuid(), longReason);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }
}
