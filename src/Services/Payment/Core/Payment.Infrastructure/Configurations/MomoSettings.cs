namespace Payment.Infrastructure.Configurations;

public class MomoSettings
{
    public const string SectionName = "Momo";
    public string PartnerCode { get; set; } = string.Empty;
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string ApiEnpoint { get; set; } = string.Empty;
    public string NotifyUrl { get; set; } = string.Empty;
    public string RequestType { get; set; } = "captureWallet";
    public string PartnerName { get; set; } = "Hieu";
    public string? StoreId { get; set; }
    public int ExpiryInMinutes { get; set; } = 15;
}
