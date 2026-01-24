using Payment.Application.Models.Results;

namespace PaymentUnitTest.Application.Models;

[TestFixture]
[Category("Unit")]
public class VnPayCallbackResultTests
{
    [Test]
    public void FromVnPayResponse_WithValidSuccessResponse_ShouldReturnSuccessResult()
    {
        // Arrange
        var vnpParams = new Dictionary<string, string>
        {
            { "vnp_ResponseCode", "00" },
            { "vnp_TransactionStatus", "00" },
            { "vnp_TxnRef", "TXN123456789" },
            { "vnp_TransactionNo", "VNP123456789" },
            { "vnp_Amount", "10000000" }, // 100,000 VND in smallest unit
            { "vnp_BankCode", "NCB" },
            { "vnp_BankTranNo", "BANK123" },
            { "vnp_PayDate", "20260125123456" }
        };

        // Act
        var result = VnPayCallbackResult.FromVnPayResponse(vnpParams, isValidSignature: true);

        // Assert
        result.IsValid.Should().BeTrue();
        result.IsSuccess.Should().BeTrue();
        result.TransactionId.Should().Be("TXN123456789");
        result.VnPayTransactionNo.Should().Be("VNP123456789");
        result.Amount.Should().Be(100000m);
        result.BankCode.Should().Be("NCB");
        result.BankTransactionNo.Should().Be("BANK123");
        result.PayDate.Should().Be("20260125123456");
        result.ResponseCode.Should().Be("00");
        result.TransactionStatus.Should().Be("00");
    }

    [Test]
    public void FromVnPayResponse_WithFailedResponse_ShouldReturnFailedResult()
    {
        // Arrange
        var vnpParams = new Dictionary<string, string>
        {
            { "vnp_ResponseCode", "24" }, // Customer cancelled
            { "vnp_TransactionStatus", "02" },
            { "vnp_TxnRef", "TXN123456789" },
            { "vnp_Amount", "10000000" }
        };

        // Act
        var result = VnPayCallbackResult.FromVnPayResponse(vnpParams, isValidSignature: true);

        // Assert
        result.IsValid.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.ResponseCode.Should().Be("24");
    }

    [Test]
    public void FromVnPayResponse_WithInvalidSignature_ShouldReturnInvalidResult()
    {
        // Arrange
        var vnpParams = new Dictionary<string, string>
        {
            { "vnp_ResponseCode", "00" },
            { "vnp_TransactionStatus", "00" },
            { "vnp_TxnRef", "TXN123456789" }
        };

        // Act
        var result = VnPayCallbackResult.FromVnPayResponse(vnpParams, isValidSignature: false);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Message.Should().Be("Invalid signature");
    }

    [Test]
    public void Invalid_ShouldCreateInvalidResult()
    {
        // Arrange
        var message = "Test error message";

        // Act
        var result = VnPayCallbackResult.Invalid(message);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Message.Should().Be(message);
    }

    [Test]
    public void FromVnPayResponse_ShouldBuildRawDataString()
    {
        // Arrange
        var vnpParams = new Dictionary<string, string>
        {
            { "vnp_ResponseCode", "00" },
            { "vnp_TransactionStatus", "00" },
            { "vnp_TxnRef", "TXN123" },
            { "vnp_Amount", "5000000" }
        };

        // Act
        var result = VnPayCallbackResult.FromVnPayResponse(vnpParams, isValidSignature: true);

        // Assert
        result.RawData.Should().NotBeNullOrEmpty();
        result.RawData.Should().Contain("vnp_ResponseCode=00");
        result.RawData.Should().Contain("vnp_TxnRef=TXN123");
    }

    [Test]
    public void FromVnPayResponse_WithMissingFields_ShouldHandleGracefully()
    {
        // Arrange
        var vnpParams = new Dictionary<string, string>
        {
            { "vnp_ResponseCode", "00" },
            { "vnp_TransactionStatus", "00" },
            { "vnp_Amount", "1000000" }
            // Missing vnp_TxnRef, vnp_TransactionNo, etc.
        };

        // Act
        var result = VnPayCallbackResult.FromVnPayResponse(vnpParams, isValidSignature: true);

        // Assert
        result.IsValid.Should().BeTrue();
        result.TransactionId.Should().BeNull();
        result.VnPayTransactionNo.Should().BeNull();
        result.BankCode.Should().BeNull();
    }
}
