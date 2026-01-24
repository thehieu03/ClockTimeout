namespace Payment.Infrastructure.Configurations;

public class VnPaySettings
{
    public const string SectionName = "VnPay";
    public string TmnCode { get; set; } = string.Empty;
    public string HashSecret { get; set; } = string.Empty;
    public string PaymentUrl { get; set; } = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
    public string ApiUrl { get; set; } = "https://sandbox.vnpayment.vn/merchant_webapi/api/transaction";
    public string ReturnUrl { get; set; } = string.Empty;
    public string Version { get; set; } = "2.1.0";
    public string Locale { get; set; } = "vn";
    public string CurrencyCode { get; set; } = "VND";
}
