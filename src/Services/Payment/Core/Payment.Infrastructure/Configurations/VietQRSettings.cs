namespace Payment.Infrastructure.Configurations;

/// <summary>
/// VietQR configuration settings
/// </summary>
public class VietQRSettings
{
    public const string SectionName = "VietQR";

    /// <summary>
    /// VietQR API URL for generating QR images
    /// </summary>
    public string ApiUrl { get; set; } = "https://api.vietqr.io/image/";

    /// <summary>
    /// Bank BIN code (e.g., 970422 for MB Bank)
    /// </summary>
    public string BankBin { get; set; } = string.Empty;

    /// <summary>
    /// Bank account number
    /// </summary>
    public string AccountNo { get; set; } = string.Empty;

    /// <summary>
    /// Account holder name
    /// </summary>
    public string AccountName { get; set; } = string.Empty;

    /// <summary>
    /// VietQR template ID for QR image styling
    /// </summary>
    public string TemplateId { get; set; } = string.Empty;
}
