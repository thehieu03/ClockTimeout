namespace Payment.Infrastructure.Configurations;

public class MomoSettings
{
    public const string SectionName = "Momo";
    public string PartnerCode { get; set; } = string.Empty;
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string ApiEndpoint { get; set; } = "https://test-payment.momo.vn/v2/gateway/api";
    public string ReturnUrl { get; set; } = string.Empty;
    public string NotifyUrl { get; set; } = string.Empty;

    /// <summary>
    /// Request Type: captureWallet, payWithATM, payWithCC
    /// </summary>
    public string RequestType { get; set; } = "captureWallet";

    /// <summary>
    /// Partner Name to display on Momo
    /// </summary>
    public string PartnerName { get; set; } = "Progcoder Shop";

    /// <summary>
    /// Store ID (optional)
    /// </summary>
    public string? StoreId { get; set; }

    /// <summary>
    /// Payment expiry time in minutes
    /// </summary>
    public int ExpiryInMinutes { get; set; } = 15;
}
