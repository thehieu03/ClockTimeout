namespace PaymentUnitTest.Domain;

[TestFixture]
[Category("Unit")]
public class PaymentEntityTests
{
    [Test]
    public void Create_ShouldCreatePaymentWithPendingStatus()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var amount = 100.50m;
        var method = PaymentMethod.VnPay;

        // Act
        var payment = PaymentEntity.Create(orderId, amount, method);

        // Assert
        payment.Should().NotBeNull();
        payment.Id.Should().NotBeEmpty();
        payment.OrderId.Should().Be(orderId);
        payment.Amount.Should().Be(amount);
        payment.Method.Should().Be(method);
        payment.Status.Should().Be(PaymentStatus.Pending);
        payment.CreatedOnUtc.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Test]
    public void Create_WithCreatedBy_ShouldSetCreatedBy()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var createdBy = "test-user@example.com";

        // Act
        var payment = PaymentEntity.Create(orderId, 50m, PaymentMethod.Momo, createdBy);

        // Assert
        payment.CreatedBy.Should().Be(createdBy);
    }

    [Test]
    public void Complete_ShouldSetStatusToCompleted()
    {
        // Arrange
        var payment = PaymentEntity.Create(Guid.NewGuid(), 100m, PaymentMethod.Stipe);
        var transactionId = "TXN-123456";

        // Act
        payment.Complete(transactionId);

        // Assert
        payment.Status.Should().Be(PaymentStatus.Completed);
        payment.TransactionId.Should().Be(transactionId);
        payment.LastModifiedOnUtc.Should().NotBeNull();
    }

    [Test]
    public void Complete_WithModifiedBy_ShouldSetLastModifiedBy()
    {
        // Arrange
        var payment = PaymentEntity.Create(Guid.NewGuid(), 100m, PaymentMethod.Cod);
        var modifiedBy = "admin@example.com";

        // Act
        payment.Complete("TXN-789", modifiedBy);

        // Assert
        payment.LastModifiedBy.Should().Be(modifiedBy);
    }

    [Test]
    public void Complete_WhenNotPending_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var payment = PaymentEntity.Create(Guid.NewGuid(), 100m, PaymentMethod.VnPay);
        payment.Complete("TXN-123");

        // Act
        var act = () => payment.Complete("TXN-456");

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Only pending payments can be completed.");
    }

    [Test]
    public void MarkAsFailed_ShouldSetStatusToFailed()
    {
        // Arrange
        var payment = PaymentEntity.Create(Guid.NewGuid(), 100m, PaymentMethod.Momo);
        var errorMessage = "Insufficient funds";

        // Act
        payment.MarkAsFailed(errorMessage);

        // Assert
        payment.Status.Should().Be(PaymentStatus.Failed);
        payment.ErrorMessage.Should().Be(errorMessage);
        payment.LastModifiedOnUtc.Should().NotBeNull();
    }

    [Test]
    public void MarkAsFailed_WhenNotPending_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var payment = PaymentEntity.Create(Guid.NewGuid(), 100m, PaymentMethod.VnPay);
        payment.MarkAsFailed("First failure");

        // Act
        var act = () => payment.MarkAsFailed("Second failure");

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Only pending payments can be marked as failed.");
    }

    [Test]
    public void Refund_ShouldSetStatusToRefunded()
    {
        // Arrange
        var payment = PaymentEntity.Create(Guid.NewGuid(), 100m, PaymentMethod.Stipe);
        payment.Complete("TXN-123");
        var refundReason = "Customer requested refund";

        // Act
        payment.Refund(refundReason);

        // Assert
        payment.Status.Should().Be(PaymentStatus.Refunded);
        payment.RefundReason.Should().Be(refundReason);
    }

    [Test]
    public void Refund_WithRefundTransactionId_ShouldSetRefundTransactionId()
    {
        // Arrange
        var payment = PaymentEntity.Create(Guid.NewGuid(), 100m, PaymentMethod.Paypay);
        payment.Complete("TXN-123");
        var refundTransactionId = "REFUND-456";

        // Act
        payment.Refund("Refund reason", refundTransactionId);

        // Assert
        payment.RefundTransactionId.Should().Be(refundTransactionId);
    }

    [Test]
    public void Refund_WhenNotCompleted_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var payment = PaymentEntity.Create(Guid.NewGuid(), 100m, PaymentMethod.VnPay);

        // Act
        var act = () => payment.Refund("Refund reason");

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Only completed payments can be refunded.");
    }

    [Test]
    public void Refund_WhenFailed_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var payment = PaymentEntity.Create(Guid.NewGuid(), 100m, PaymentMethod.VnPay);
        payment.MarkAsFailed("Payment failed");

        // Act
        var act = () => payment.Refund("Refund reason");

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Only completed payments can be refunded.");
    }
}
