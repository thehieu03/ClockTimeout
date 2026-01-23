using AutoMapper;
using BuildingBlocks.Extensions;
using Payment.Application.Mappings;

namespace PaymentUnitTest.Application.Queries;

[TestFixture]
[Category("Unit")]
public class GetPaymentByIdQueryTests
{
    private Mock<IUnitOfWork> _mockUnitOfWork;
    private Mock<IPaymentRepository> _mockPaymentRepository;
    private IMapper _mapper;
    private GetPaymentByIdQueryHandler _handler;

    [SetUp]
    public void Setup()
    {
        _mockPaymentRepository = new Mock<IPaymentRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockUnitOfWork.Setup(u => u.Payments).Returns(_mockPaymentRepository.Object);

        var config = new MapperConfiguration(cfg => cfg.AddProfile<PaymentMappingProfile>());
        _mapper = config.CreateMapper();

        _handler = new GetPaymentByIdQueryHandler(_mockUnitOfWork.Object, _mapper);
    }

    [Test]
    public async Task Handle_ShouldReturnPaymentDto_WhenPaymentExists()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var payment = PaymentEntity.Create(orderId, 150.50m, PaymentMethod.VnPay);
        typeof(PaymentEntity).GetProperty("Id")!.SetValue(payment, paymentId);

        _mockPaymentRepository
            .Setup(r => r.GetByIdAsync(paymentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        var query = new GetPaymentByIdQuery(paymentId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(paymentId);
        result.OrderId.Should().Be(orderId);
        result.Amount.Should().Be(150.50m);
        result.Method.Should().Be(PaymentMethod.VnPay);
        result.Status.Should().Be(PaymentStatus.Pending);
        result.StatusName.Should().Be("Pending");
        result.MethodName.Should().Be("VnPay");
    }

    [Test]
    public async Task Handle_ShouldThrowNotFoundException_WhenPaymentDoesNotExist()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        _mockPaymentRepository
            .Setup(r => r.GetByIdAsync(paymentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PaymentEntity?)null);

        var query = new GetPaymentByIdQuery(paymentId);

        // Act
        var act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}

[TestFixture]
[Category("Unit")]
public class GetPaymentByOrderIdQueryTests
{
    private Mock<IUnitOfWork> _mockUnitOfWork;
    private Mock<IPaymentRepository> _mockPaymentRepository;
    private IMapper _mapper;
    private GetPaymentByOrderIdQueryHandler _handler;

    [SetUp]
    public void Setup()
    {
        _mockPaymentRepository = new Mock<IPaymentRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockUnitOfWork.Setup(u => u.Payments).Returns(_mockPaymentRepository.Object);

        var config = new MapperConfiguration(cfg => cfg.AddProfile<PaymentMappingProfile>());
        _mapper = config.CreateMapper();

        _handler = new GetPaymentByOrderIdQueryHandler(_mockUnitOfWork.Object, _mapper);
    }

    [Test]
    public async Task Handle_ShouldReturnPaymentDto_WhenPaymentExists()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var payment = PaymentEntity.Create(orderId, 200m, PaymentMethod.Momo);

        _mockPaymentRepository
            .Setup(r => r.GetByOrderIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        var query = new GetPaymentByOrderIdQuery(orderId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.OrderId.Should().Be(orderId);
        result.Amount.Should().Be(200m);
    }

    [Test]
    public async Task Handle_ShouldThrowNotFoundException_WhenNoPaymentForOrder()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        _mockPaymentRepository
            .Setup(r => r.GetByOrderIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PaymentEntity?)null);

        var query = new GetPaymentByOrderIdQuery(orderId);

        // Act
        var act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}

[TestFixture]
[Category("Unit")]
public class GetPaymentsQueryTests
{
    private Mock<IUnitOfWork> _mockUnitOfWork;
    private Mock<IPaymentRepository> _mockPaymentRepository;
    private IMapper _mapper;
    private GetPaymentsQueryHandler _handler;

    [SetUp]
    public void Setup()
    {
        _mockPaymentRepository = new Mock<IPaymentRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockUnitOfWork.Setup(u => u.Payments).Returns(_mockPaymentRepository.Object);

        var config = new MapperConfiguration(cfg => cfg.AddProfile<PaymentMappingProfile>());
        _mapper = config.CreateMapper();

        _handler = new GetPaymentsQueryHandler(_mockUnitOfWork.Object, _mapper);
    }

    [Test]
    public async Task Handle_ShouldReturnAllPayments()
    {
        // Arrange
        var payments = new List<PaymentEntity>
        {
            PaymentEntity.Create(Guid.NewGuid(), 100m, PaymentMethod.VnPay),
            PaymentEntity.Create(Guid.NewGuid(), 200m, PaymentMethod.Momo),
            PaymentEntity.Create(Guid.NewGuid(), 300m, PaymentMethod.Stipe)
        };

        _mockPaymentRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(payments);

        var query = new GetPaymentsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(3);
        result.Select(p => p.Amount).Should().BeEquivalentTo(new[] { 100m, 200m, 300m });
    }

    [Test]
    public async Task Handle_ShouldReturnEmptyList_WhenNoPayments()
    {
        // Arrange
        _mockPaymentRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PaymentEntity>());

        var query = new GetPaymentsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }
}

[TestFixture]
[Category("Unit")]
public class GetPaymentsByStatusQueryTests
{
    private Mock<IUnitOfWork> _mockUnitOfWork;
    private Mock<IPaymentRepository> _mockPaymentRepository;
    private IMapper _mapper;
    private GetPaymentsByStatusQueryHandler _handler;

    [SetUp]
    public void Setup()
    {
        _mockPaymentRepository = new Mock<IPaymentRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockUnitOfWork.Setup(u => u.Payments).Returns(_mockPaymentRepository.Object);

        var config = new MapperConfiguration(cfg => cfg.AddProfile<PaymentMappingProfile>());
        _mapper = config.CreateMapper();

        _handler = new GetPaymentsByStatusQueryHandler(_mockUnitOfWork.Object, _mapper);
    }

    [Test]
    public async Task Handle_ShouldReturnPaymentsWithMatchingStatus()
    {
        // Arrange
        var pendingPayment = PaymentEntity.Create(Guid.NewGuid(), 100m, PaymentMethod.VnPay);
        var payments = new List<PaymentEntity> { pendingPayment };

        _mockPaymentRepository
            .Setup(r => r.GetByStatusAsync(PaymentStatus.Pending, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payments);

        var query = new GetPaymentsByStatusQuery(PaymentStatus.Pending);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result.First().Status.Should().Be(PaymentStatus.Pending);
    }

    [Test]
    public async Task Handle_ShouldReturnCompletedPayments()
    {
        // Arrange
        var payment = PaymentEntity.Create(Guid.NewGuid(), 100m, PaymentMethod.Momo);
        payment.Complete("TXN-123");
        var payments = new List<PaymentEntity> { payment };

        _mockPaymentRepository
            .Setup(r => r.GetByStatusAsync(PaymentStatus.Completed, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payments);

        var query = new GetPaymentsByStatusQuery(PaymentStatus.Completed);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result.First().Status.Should().Be(PaymentStatus.Completed);
        result.First().TransactionId.Should().Be("TXN-123");
    }
}
