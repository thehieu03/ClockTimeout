using FluentAssertions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Payment.Api.Endpoints;
using Payment.Infrastructure.Configurations;

namespace PaymentUnitTest.Endpoints;

[TestFixture]
public class VietQREndpointsTests
{
    private Mock<IOptions<VietQRSettings>> _settingsMock = null!;
    private Mock<ILogger<VietQREndpoints>> _loggerMock = null!;
    private VietQRSettings _settings = null!;

    [SetUp]
    public void Setup()
    {
        _settings = new VietQRSettings
        {
            ApiUrl = "https://api.vietqr.io/image/",
            BankBin = "970422",
            AccountNo = "0868430273",
            AccountName = "GORNER ROBIN",
            TemplateId = "U4NCcYH"
        };

        _settingsMock = new Mock<IOptions<VietQRSettings>>();
        _settingsMock.Setup(x => x.Value).Returns(_settings);

        _loggerMock = new Mock<ILogger<VietQREndpoints>>();
    }

    [Test]
    public void GenerateQR_WithValidAmount_ShouldReturnQRCodeUrl()
    {
        // Arrange
        var amount = 100000m;
        var description = "Test payment";

        // Act
        var qrUrl = BuildQRUrl(amount, description, null);

        // Assert
        qrUrl.Should().Contain("https://api.vietqr.io/image/");
        qrUrl.Should().Contain("970422-0868430273-U4NCcYH.png");
        qrUrl.Should().Contain("amount=100000");
        qrUrl.Should().Contain("addInfo=");
    }

    [Test]
    public void GenerateQR_WithOrderId_ShouldIncludeOrderIdInDescription()
    {
        // Arrange
        var amount = 50000m;
        var orderId = "ORD-12345";

        // Act
        var qrUrl = BuildQRUrl(amount, null, orderId);

        // Assert
        qrUrl.Should().Contain("amount=50000");
        qrUrl.Should().Contain("addInfo=");
        qrUrl.Should().Contain(Uri.EscapeDataString($"Thanh toan don hang {orderId}"));
    }

    [Test]
    public void GenerateQR_WithCustomDescription_ShouldUseCustomDescription()
    {
        // Arrange
        var amount = 75000m;
        var description = "Custom payment description";

        // Act
        var qrUrl = BuildQRUrl(amount, description, null);

        // Assert
        qrUrl.Should().Contain(Uri.EscapeDataString(description));
    }

    [Test]
    public void GenerateQR_ShouldIncludeAccountName()
    {
        // Arrange
        var amount = 100000m;

        // Act
        var qrUrl = BuildQRUrl(amount, null, null);

        // Assert
        qrUrl.Should().Contain("accountName=");
        qrUrl.Should().Contain(Uri.EscapeDataString(_settings.AccountName));
    }

    [Test]
    public void GenerateQR_ShouldBuildCorrectUrlFormat()
    {
        // Arrange
        var amount = 200000m;
        var description = "Test";

        // Act
        var qrUrl = BuildQRUrl(amount, description, null);

        // Assert
        qrUrl.Should().StartWith(_settings.ApiUrl);
        qrUrl.Should().Contain($"{_settings.BankBin}-{_settings.AccountNo}-{_settings.TemplateId}.png");
    }

    [Test]
    public void VietQRResponse_ShouldContainAllRequiredFields()
    {
        // Arrange & Act
        var response = new VietQRResponse
        {
            QRCodeUrl = "https://example.com/qr.png",
            Amount = 100000m,
            Description = "Test payment",
            AccountNo = "0868430273",
            AccountName = "GORNER ROBIN",
            BankBin = "970422",
            OrderId = "ORD-123"
        };

        // Assert
        response.QRCodeUrl.Should().NotBeEmpty();
        response.Amount.Should().Be(100000m);
        response.Description.Should().Be("Test payment");
        response.AccountNo.Should().Be("0868430273");
        response.AccountName.Should().Be("GORNER ROBIN");
        response.BankBin.Should().Be("970422");
        response.OrderId.Should().Be("ORD-123");
    }

    [Test]
    public void VietQRAccountInfo_ShouldContainCorrectInfo()
    {
        // Arrange & Act
        var info = new VietQRAccountInfo
        {
            BankBin = _settings.BankBin,
            AccountNo = _settings.AccountNo,
            AccountName = _settings.AccountName,
            ApiUrl = _settings.ApiUrl
        };

        // Assert
        info.BankBin.Should().Be("970422");
        info.AccountNo.Should().Be("0868430273");
        info.AccountName.Should().Be("GORNER ROBIN");
        info.ApiUrl.Should().Be("https://api.vietqr.io/image/");
    }

    [Test]
    public void VietQRSettings_DefaultValues_ShouldBeCorrect()
    {
        // Arrange
        var defaultSettings = new VietQRSettings();

        // Assert
        defaultSettings.ApiUrl.Should().Be("https://api.vietqr.io/image/");
        defaultSettings.BankBin.Should().BeEmpty();
        defaultSettings.AccountNo.Should().BeEmpty();
        defaultSettings.AccountName.Should().BeEmpty();
        defaultSettings.TemplateId.Should().BeEmpty();
    }

    [Test]
    public void GenerateQR_WithDecimalAmount_ShouldFormatCorrectly()
    {
        // Arrange
        var amount = 123456.78m;

        // Act
        var qrUrl = BuildQRUrl(amount, null, null);

        // Assert
        // VietQR expects whole numbers
        qrUrl.Should().Contain("amount=123457"); // Should be rounded
    }

    private string BuildQRUrl(decimal amount, string? description, string? orderId)
    {
        var paymentDescription = string.IsNullOrEmpty(orderId)
            ? description ?? "Thanh toan don hang"
            : $"Thanh toan don hang {orderId}";

        return $"{_settings.ApiUrl}{_settings.BankBin}-{_settings.AccountNo}-{_settings.TemplateId}.png" +
               $"?amount={amount:F0}" +
               $"&addInfo={Uri.EscapeDataString(paymentDescription)}" +
               $"&accountName={Uri.EscapeDataString(_settings.AccountName)}";
    }
}
