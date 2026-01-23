using BuildingBlocks.Extensions;

namespace PaymentUnitTest.Application.Commands;

[TestFixture]
[Category("Unit")]
public class CompletePaymentCommandTests
{
    private Mock<IUnitOfWork> _mockUnitOfWork;
    private Mock<IPaymentRepository> _mockPaymentRepository;
    private CompletePaymentCommandHandler _handler;

    [SetUp]
    public void Setup()
    {
        _mockPaymentRepository = new Mock<IPaymentRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockUnitOfWork.Setup(u => u.Payments).Returns(_mockPaymentRepository.Object);

        _handler = new CompletePaymentCommandHandler(_mockUnitOfWork.Object);
    }

    [Test]
    public async Task Handle_ShouldCompletePayment()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var transactionId = "TXN-123456";
        var payment = PaymentEntity.Create(Guid.NewGuid(), 100m, PaymentMethod.VnPay);

        // Use reflection to set Id since it's set internally
        typeof(PaymentEntity).GetProperty("Id")!.SetValue(payment, paymentId);

        _mockPaymentRepository
            .Setup(r => r.GetByIdAsync(paymentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = new CompletePaymentCommand(paymentId, transactionId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        payment.Status.Should().Be(PaymentStatus.Completed);
        payment.TransactionId.Should().Be(transactionId);
        _mockPaymentRepository.Verify(r => r.Update(payment), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_WhenPaymentNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        _mockPaymentRepository
            .Setup(r => r.GetByIdAsync(paymentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PaymentEntity?)null);

        var command = new CompletePaymentCommand(paymentId, "TXN-123");

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task Handle_WhenPaymentAlreadyCompleted_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var payment = PaymentEntity.Create(Guid.NewGuid(), 100m, PaymentMethod.VnPay);
        payment.Complete("TXN-OLD");

        _mockPaymentRepository
            .Setup(r => r.GetByIdAsync(paymentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        var command = new CompletePaymentCommand(paymentId, "TXN-NEW");

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}

[TestFixture]
[Category("Unit")]
public class CompletePaymentCommandValidatorTests
{
    private CompletePaymentCommandValidator _validator;

    [SetUp]
    public void Setup()
    {
        _validator = new CompletePaymentCommandValidator();
    }

    [Test]
    public void Validate_WithValidCommand_ShouldPass()
    {
        // Arrange
        var command = new CompletePaymentCommand(Guid.NewGuid(), "TXN-123");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Test]
    public void Validate_WithEmptyPaymentId_ShouldFail()
    {
        // Arrange
        var command = new CompletePaymentCommand(Guid.Empty, "TXN-123");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Test]
    public void Validate_WithEmptyTransactionId_ShouldFail()
    {
        // Arrange
        var command = new CompletePaymentCommand(Guid.NewGuid(), "");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }
}
