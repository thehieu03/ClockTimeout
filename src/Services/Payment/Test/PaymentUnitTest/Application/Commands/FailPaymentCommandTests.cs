using BuildingBlocks.Extensions;

namespace PaymentUnitTest.Application.Commands;

[TestFixture]
[Category("Unit")]
public class FailPaymentCommandTests
{
    private Mock<IUnitOfWork> _mockUnitOfWork;
    private Mock<IPaymentRepository> _mockPaymentRepository;
    private FailPaymentCommandHandler _handler;

    [SetUp]
    public void Setup()
    {
        _mockPaymentRepository = new Mock<IPaymentRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockUnitOfWork.Setup(u => u.Payments).Returns(_mockPaymentRepository.Object);

        _handler = new FailPaymentCommandHandler(_mockUnitOfWork.Object);
    }

    [Test]
    public async Task Handle_ShouldMarkPaymentAsFailed()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var errorCode = "CARD_DECLINED";
        var errorMessage = "Card declined";
        var payment = PaymentEntity.Create(Guid.NewGuid(), 100m, PaymentMethod.Stipe);

        _mockPaymentRepository
            .Setup(r => r.GetByIdAsync(paymentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = new FailPaymentCommand(paymentId, errorCode, errorMessage);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        payment.Status.Should().Be(PaymentStatus.Failed);
        payment.ErrorCode.Should().Be(errorCode);
        payment.ErrorMessage.Should().Be(errorMessage);
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

        var command = new FailPaymentCommand(paymentId, "ERROR_CODE", "Error message");

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}

[TestFixture]
[Category("Unit")]
public class FailPaymentCommandValidatorTests
{
    private FailPaymentCommandValidator _validator;

    [SetUp]
    public void Setup()
    {
        _validator = new FailPaymentCommandValidator();
    }

    [Test]
    public void Validate_WithValidCommand_ShouldPass()
    {
        // Arrange
        var command = new FailPaymentCommand(Guid.NewGuid(), "ERROR_CODE", "Payment failed");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Test]
    public void Validate_WithEmptyErrorMessage_ShouldFail()
    {
        // Arrange
        var command = new FailPaymentCommand(Guid.NewGuid(), "ERROR_CODE", "");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Test]
    public void Validate_WithTooLongErrorMessage_ShouldFail()
    {
        // Arrange
        var longMessage = new string('x', 501);
        var command = new FailPaymentCommand(Guid.NewGuid(), "ERROR_CODE", longMessage);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Test]
    public void Validate_WithEmptyErrorCode_ShouldFail()
    {
        // Arrange
        var command = new FailPaymentCommand(Guid.NewGuid(), "", "Error message");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }
}
