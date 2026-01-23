using AutoMapper;
using Payment.Application.Mappings;

namespace PaymentUnitTest.Application.Commands;

[TestFixture]
[Category("Unit")]
public class CreatePaymentCommandTests
{
    private Mock<IUnitOfWork> _mockUnitOfWork;
    private Mock<IPaymentRepository> _mockPaymentRepository;
    private CreatePaymentCommandHandler _handler;

    [SetUp]
    public void Setup()
    {
        _mockPaymentRepository = new Mock<IPaymentRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockUnitOfWork.Setup(u => u.Payments).Returns(_mockPaymentRepository.Object);

        _handler = new CreatePaymentCommandHandler(_mockUnitOfWork.Object);
    }

    [Test]
    public async Task Handle_ShouldCreatePaymentAndReturnId()
    {
        // Arrange
        var dto = new CreatePaymentDto
        {
            OrderId = Guid.NewGuid(),
            Amount = 150.00m,
            Method = PaymentMethod.VnPay
        };
        var command = new CreatePaymentCommand(dto, "test-user");

        _mockPaymentRepository
            .Setup(r => r.AddAsync(It.IsAny<PaymentEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        _mockPaymentRepository.Verify(r => r.AddAsync(It.Is<PaymentEntity>(p =>
            p.OrderId == dto.OrderId &&
            p.Amount == dto.Amount &&
            p.Method == dto.Method &&
            p.Status == PaymentStatus.Pending),
            It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_ShouldSetCreatedBy()
    {
        // Arrange
        var dto = new CreatePaymentDto
        {
            OrderId = Guid.NewGuid(),
            Amount = 200.00m,
            Method = PaymentMethod.Momo
        };
        var performedBy = "admin@example.com";
        var command = new CreatePaymentCommand(dto, performedBy);

        PaymentEntity? capturedPayment = null;
        _mockPaymentRepository
            .Setup(r => r.AddAsync(It.IsAny<PaymentEntity>(), It.IsAny<CancellationToken>()))
            .Callback<PaymentEntity, CancellationToken>((p, _) => capturedPayment = p)
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedPayment.Should().NotBeNull();
        capturedPayment!.CreatedBy.Should().Be(performedBy);
    }
}

[TestFixture]
[Category("Unit")]
public class CreatePaymentCommandValidatorTests
{
    private CreatePaymentCommandValidator _validator;

    [SetUp]
    public void Setup()
    {
        _validator = new CreatePaymentCommandValidator();
    }

    [Test]
    public void Validate_WithValidDto_ShouldPass()
    {
        // Arrange
        var command = new CreatePaymentCommand(new CreatePaymentDto
        {
            OrderId = Guid.NewGuid(),
            Amount = 100m,
            Method = PaymentMethod.VnPay
        });

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Test]
    public void Validate_WithEmptyOrderId_ShouldFail()
    {
        // Arrange
        var command = new CreatePaymentCommand(new CreatePaymentDto
        {
            OrderId = Guid.Empty,
            Amount = 100m,
            Method = PaymentMethod.VnPay
        });

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("OrderId"));
    }

    [Test]
    public void Validate_WithZeroAmount_ShouldFail()
    {
        // Arrange
        var command = new CreatePaymentCommand(new CreatePaymentDto
        {
            OrderId = Guid.NewGuid(),
            Amount = 0m,
            Method = PaymentMethod.VnPay
        });

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("Amount"));
    }

    [Test]
    public void Validate_WithNegativeAmount_ShouldFail()
    {
        // Arrange
        var command = new CreatePaymentCommand(new CreatePaymentDto
        {
            OrderId = Guid.NewGuid(),
            Amount = -50m,
            Method = PaymentMethod.VnPay
        });

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }
}
