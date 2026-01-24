using Payment.Infrastructure.Gateways.VnPay;

namespace PaymentUnitTest.Infrastructure;

[TestFixture]
[Category("Unit")]
public class VnPayHelperTests
{
    private const string TestHashSecret = "XNBCJFAKAZQSGTARRLGCHVZWCIOIGSHN";
    private const string TestTmnCode = "CGXZLS0Z";
    private const string TestBaseUrl = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";

    [Test]
    public void BuildPaymentUrl_ShouldReturnValidUrl()
    {
        // Arrange
        var txnRef = "TEST123456789012";
        var amount = 100000m;
        var orderInfo = "Test payment";
        var returnUrl = "https://example.com/callback";
        var ipAddress = "127.0.0.1";

        // Act
        var result = VnPayHelper.BuildPaymentUrl(
            TestBaseUrl,
            TestTmnCode,
            TestHashSecret,
            txnRef,
            amount,
            orderInfo,
            returnUrl,
            ipAddress);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().StartWith(TestBaseUrl);
        result.Should().Contain("vnp_TxnRef=TEST123456789012");
        result.Should().Contain("vnp_Amount=10000000"); // 100000 * 100
        result.Should().Contain("vnp_TmnCode=" + TestTmnCode);
        result.Should().Contain("vnp_SecureHash=");
    }

    [Test]
    public void BuildPaymentUrl_WithBankCode_ShouldIncludeBankCode()
    {
        // Arrange
        var txnRef = "TEST123456789012";
        var amount = 50000m;
        var bankCode = "NCB";

        // Act
        var result = VnPayHelper.BuildPaymentUrl(
            TestBaseUrl,
            TestTmnCode,
            TestHashSecret,
            txnRef,
            amount,
            "Order info",
            "https://example.com/callback",
            "127.0.0.1",
            bankCode: bankCode);

        // Assert
        result.Should().Contain("vnp_BankCode=NCB");
    }

    [Test]
    public void ParseQueryString_ShouldParseValidQueryString()
    {
        // Arrange
        var queryString = "?vnp_TxnRef=123&vnp_Amount=10000000&vnp_ResponseCode=00";

        // Act
        var result = VnPayHelper.ParseQueryString(queryString);

        // Assert
        result.Should().HaveCount(3);
        result["vnp_TxnRef"].Should().Be("123");
        result["vnp_Amount"].Should().Be("10000000");
        result["vnp_ResponseCode"].Should().Be("00");
    }

    [Test]
    public void ParseQueryString_WithEmptyString_ShouldReturnEmptyDictionary()
    {
        // Act
        var result = VnPayHelper.ParseQueryString("");

        // Assert
        result.Should().BeEmpty();
    }

    [Test]
    public void ParseQueryString_WithNullString_ShouldReturnEmptyDictionary()
    {
        // Act
        var result = VnPayHelper.ParseQueryString(null!);

        // Assert
        result.Should().BeEmpty();
    }

    [Test]
    public void ValidateSignature_WithValidSignature_ShouldReturnTrue()
    {
        // Arrange - Build a payment URL first to get a valid signature
        var txnRef = "VALIDTEST12345678";
        var paymentUrl = VnPayHelper.BuildPaymentUrl(
            TestBaseUrl,
            TestTmnCode,
            TestHashSecret,
            txnRef,
            50000m,
            "Test order",
            "https://example.com/callback",
            "127.0.0.1");

        // Parse the URL to get params
        var queryString = paymentUrl.Substring(paymentUrl.IndexOf('?'));
        var vnpParams = VnPayHelper.ParseQueryString(queryString);
        var secureHash = vnpParams["vnp_SecureHash"];

        // Act
        var isValid = VnPayHelper.ValidateSignature(vnpParams, secureHash, TestHashSecret);

        // Assert
        isValid.Should().BeTrue();
    }

    [Test]
    public void ValidateSignature_WithInvalidSignature_ShouldReturnFalse()
    {
        // Arrange
        var vnpParams = new Dictionary<string, string>
        {
            { "vnp_TxnRef", "123" },
            { "vnp_Amount", "10000000" },
            { "vnp_ResponseCode", "00" }
        };
        var invalidHash = "invalid_hash_12345";

        // Act
        var isValid = VnPayHelper.ValidateSignature(vnpParams, invalidHash, TestHashSecret);

        // Assert
        isValid.Should().BeFalse();
    }

    [Test]
    [TestCase("00", "Giao dịch thành công")]
    [TestCase("07", "Trừ tiền thành công. Giao dịch bị nghi ngờ (liên quan tới lừa đảo, giao dịch bất thường)")]
    [TestCase("24", "Khách hàng hủy giao dịch")]
    [TestCase("51", "Tài khoản không đủ số dư")]
    [TestCase("99", "Lỗi không xác định")]
    public void GetResponseMessage_ShouldReturnCorrectMessage(string code, string expectedMessage)
    {
        // Act
        var result = VnPayHelper.GetResponseMessage(code);

        // Assert
        result.Should().Be(expectedMessage);
    }

    [Test]
    public void GetResponseMessage_WithUnknownCode_ShouldReturnUnknownMessage()
    {
        // Act
        var result = VnPayHelper.GetResponseMessage("XYZ");

        // Assert
        result.Should().Contain("XYZ");
    }

    [Test]
    [TestCase("00", "00", true)]
    [TestCase("00", "01", false)]
    [TestCase("01", "00", false)]
    [TestCase("24", "00", false)]
    public void IsSuccessTransaction_ShouldReturnCorrectResult(
        string responseCode, 
        string transactionStatus, 
        bool expected)
    {
        // Act
        var result = VnPayHelper.IsSuccessTransaction(responseCode, transactionStatus);

        // Assert
        result.Should().Be(expected);
    }
}
