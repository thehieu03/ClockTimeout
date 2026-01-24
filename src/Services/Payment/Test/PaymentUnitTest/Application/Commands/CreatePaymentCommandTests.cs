using AutoMapper;
using Common.ValueObjects;
using Microsoft.Extensions.Logging;
using Payment.Application.Mappings;

namespace PaymentUnitTest.Application.Commands;

[TestFixture]
[Category("Unit")]
public class CreatePaymentCommandTests
{
    private Mock<IUnitOfWork> _mockUnitOfWork;
    private Mock<IPaymentRepository> _mockPaymentRepository;
    private Mock<IMapper> _mockMapper;
    private Mock<ILogger<CreatePaymentCommandHandler>> _mockLogger;
    private CreatePaymentCommandHandler _handler;

    [SetUp]
    public void Setup()
    {
        _mockPaymentRepository = new Mock<IPaymentRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<CreatePaymentCommandHandler>>();

        _handler = new CreatePaymentCommandHandler(
            _mockPaymentRepository.Object,
            _mockUnitOfWork.Object,
            _mockMapper.Object,
            _mockLogger.Object);
    }

    [Test]
    public async Task Handle_ShouldCreatePaymentAndReturnDto()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var amount = 150.00m;
        var method = PaymentMethod.VnPay;
        var actor = Actor.User("test-user");
        var command = new CreatePaymentCommand(orderId, amount, method, actor);

        var expectedDto = new PaymentDto
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            Amount = amount,
            Method = method,
            Status = PaymentStatus.Pending
        };

        _mockPaymentRepository
            .Setup(r => r.GetByOrderIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PaymentEntity?)null);

        _mockPaymentRepository
            .Setup(r => r.AddAsync(It.IsAny<PaymentEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _mockMapper
            .Setup(m => m.Map<PaymentDto>(It.IsAny<PaymentEntity>()))
            .Returns(expectedDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.OrderId.Should().Be(orderId);
        result.Amount.Should().Be(amount);
        _mockPaymentRepository.Verify(r => r.AddAsync(It.Is<PaymentEntity>(p =>
            p.OrderId == orderId &&
            p.Amount == amount &&
            p.Method == method &&
            p.Status == PaymentStatus.Pending),
            It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_ShouldSetCreatedBy()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var amount = 200.00m;
        var method = PaymentMethod.Momo;
        var performedBy = "admin@example.com";
        var actor = Actor.User(performedBy);
        var command = new CreatePaymentCommand(orderId, amount, method, actor);

        var expectedDto = new PaymentDto
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            Amount = amount,
            Method = method,
            Status = PaymentStatus.Pending,
            CreatedBy = performedBy
        };

        PaymentEntity? capturedPayment = null;

        _mockPaymentRepository
            .Setup(r => r.GetByOrderIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PaymentEntity?)null);

        _mockPaymentRepository
            .Setup(r => r.AddAsync(It.IsAny<PaymentEntity>(), It.IsAny<CancellationToken>()))
            .Callback<PaymentEntity, CancellationToken>((p, _) => capturedPayment = p)
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _mockMapper
            .Setup(m => m.Map<PaymentDto>(It.IsAny<PaymentEntity>()))
            .Returns(expectedDto);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedPayment.Should().NotBeNull();
        capturedPayment!.CreatedBy.Should().Be(performedBy);
    }

    [Test]
    public async Task Handle_ShouldReturnExistingPayment_WhenPendingPaymentExists()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var amount = 150.00m;
        var method = PaymentMethod.VnPay;
        var actor = Actor.User("test-user");
        var command = new CreatePaymentCommand(orderId, amount, method, actor);

        var existingPayment = PaymentEntity.Create(orderId, amount, method, "existing-user");

        var expectedDto = new PaymentDto
        {
            Id = existingPayment.Id,
            OrderId = orderId,
            Amount = amount,
            Method = method,
            Status = PaymentStatus.Pending
        };

        _mockPaymentRepository
            .Setup(r => r.GetByOrderIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPayment);

        _mockMapper
            .Setup(m => m.Map<PaymentDto>(existingPayment))
            .Returns(expectedDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(existingPayment.Id);
        _mockPaymentRepository.Verify(r => r.AddAsync(It.IsAny<PaymentEntity>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
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
    public void Validate_WithValidCommand_ShouldPass()
    {
        // Arrange
        var command = new CreatePaymentCommand(
            Guid.NewGuid(),
            100m,
            PaymentMethod.VnPay,
            Actor.User("test-user"));

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Test]
    public void Validate_WithEmptyOrderId_ShouldFail()
    {
        // Arrange
        var command = new CreatePaymentCommand(
            Guid.Empty,
            100m,
            PaymentMethod.VnPay,
            Actor.User("test-user"));

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
        var command = new CreatePaymentCommand(
            Guid.NewGuid(),
            0m,
            PaymentMethod.VnPay,
            Actor.User("test-user"));

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
        var command = new CreatePaymentCommand(
            Guid.NewGuid(),
            -50m,
            PaymentMethod.VnPay,
            Actor.User("test-user"));

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }
}
