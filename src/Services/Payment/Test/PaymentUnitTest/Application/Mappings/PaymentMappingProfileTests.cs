using AutoMapper;
using Payment.Application.Mappings;

namespace PaymentUnitTest.Application.Mappings;

[TestFixture]
[Category("Unit")]
public class PaymentMappingProfileTests
{
    private IMapper _mapper;

    [SetUp]
    public void Setup()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<PaymentMappingProfile>());
        _mapper = config.CreateMapper();
    }

    [Test]
    public void Configuration_ShouldBeValid()
    {
        // Assert
        var config = new MapperConfiguration(cfg => cfg.AddProfile<PaymentMappingProfile>());
        config.AssertConfigurationIsValid();
    }

    [Test]
    public void Map_PaymentEntity_To_PaymentDto_ShouldMapAllProperties()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var payment = PaymentEntity.Create(orderId, 150.50m, PaymentMethod.VnPay, "creator@example.com");
        var paymentId = Guid.NewGuid();
        typeof(PaymentEntity).GetProperty("Id")!.SetValue(payment, paymentId);

        // Act
        var dto = _mapper.Map<PaymentDto>(payment);

        // Assert
        dto.Id.Should().Be(paymentId);
        dto.OrderId.Should().Be(orderId);
        dto.Amount.Should().Be(150.50m);
        dto.Status.Should().Be(PaymentStatus.Pending);
        dto.StatusName.Should().Be("Pending");
        dto.Method.Should().Be(PaymentMethod.VnPay);
        dto.MethodName.Should().Be("VnPay");
        dto.CreatedBy.Should().Be("creator@example.com");
    }

    [Test]
    public void Map_CompletedPayment_ShouldIncludeTransactionId()
    {
        // Arrange
        var payment = PaymentEntity.Create(Guid.NewGuid(), 100m, PaymentMethod.Stipe);
        payment.Complete("TXN-123456", null, "admin@example.com");

        // Act
        var dto = _mapper.Map<PaymentDto>(payment);

        // Assert
        dto.Status.Should().Be(PaymentStatus.Completed);
        dto.StatusName.Should().Be("Completed");
        dto.TransactionId.Should().Be("TXN-123456");
        dto.LastModifiedBy.Should().Be("admin@example.com");
    }

    [Test]
    public void Map_FailedPayment_ShouldIncludeErrorMessage()
    {
        // Arrange
        var payment = PaymentEntity.Create(Guid.NewGuid(), 100m, PaymentMethod.Momo);
        payment.MarkAsFailed("INSUFFICIENT_FUNDS", "Insufficient funds");

        // Act
        var dto = _mapper.Map<PaymentDto>(payment);

        // Assert
        dto.Status.Should().Be(PaymentStatus.Failed);
        dto.ErrorMessage.Should().Be("Insufficient funds");
    }

    [Test]
    public void Map_RefundedPayment_ShouldIncludeRefundDetails()
    {
        // Arrange
        var payment = PaymentEntity.Create(Guid.NewGuid(), 100m, PaymentMethod.Paypay);
        payment.Complete("TXN-123");
        payment.Refund("Customer request", "REFUND-456");

        // Act
        var dto = _mapper.Map<PaymentDto>(payment);

        // Assert
        dto.Status.Should().Be(PaymentStatus.Refunded);
        dto.RefundReason.Should().Be("Customer request");
        dto.RefundTransactionId.Should().Be("REFUND-456");
    }

    [Test]
    public void Map_PaymentEntityList_To_PaymentDtoList()
    {
        // Arrange
        var payments = new List<PaymentEntity>
        {
            PaymentEntity.Create(Guid.NewGuid(), 100m, PaymentMethod.VnPay),
            PaymentEntity.Create(Guid.NewGuid(), 200m, PaymentMethod.Momo),
            PaymentEntity.Create(Guid.NewGuid(), 300m, PaymentMethod.Cod)
        };

        // Act
        var dtos = _mapper.Map<List<PaymentDto>>(payments);

        // Assert
        dtos.Should().HaveCount(3);
        dtos.Select(d => d.Amount).Should().BeEquivalentTo(new[] { 100m, 200m, 300m });
    }
}
