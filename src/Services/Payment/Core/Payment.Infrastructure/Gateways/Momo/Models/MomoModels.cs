namespace Payment.Infrastructure.Gateways.Momo.Models;

using System.Text.Json.Serialization;

/// <summary>
/// Momo IPN (Instant Payment Notification) Request
/// Docs: https://developers.momo.vn/#/docs/aiov2/?id=ki%e1%bb%83m-tra-tr%e1%ba%a1ng-th%c3%a1i-giao-d%e1%bb%8bch
/// </summary>
public class MomoIpnRequest
{
    /// <summary>
    /// Partner code provided by Momo
    /// </summary>
    [JsonPropertyName("partnerCode")]
    public string PartnerCode { get; set; } = default!;

    /// <summary>
    /// Order ID from your system (should be PaymentId.ToString())
    /// </summary>
    [JsonPropertyName("orderId")]
    public string OrderId { get; set; } = default!;

    /// <summary>
    /// Unique request ID for this transaction
    /// </summary>
    [JsonPropertyName("requestId")]
    public string RequestId { get; set; } = default!;

    /// <summary>
    /// Payment amount in VND (no decimal)
    /// </summary>
    [JsonPropertyName("amount")]
    public long Amount { get; set; }

    /// <summary>
    /// Order description
    /// </summary>
    [JsonPropertyName("orderInfo")]
    public string OrderInfo { get; set; } = default!;

    /// <summary>
    /// Order type (usually "momo_wallet")
    /// </summary>
    [JsonPropertyName("orderType")]
    public string OrderType { get; set; } = default!;

    /// <summary>
    /// Momo transaction ID (from Momo system)
    /// </summary>
    [JsonPropertyName("transId")]
    public long TransId { get; set; }

    /// <summary>
    /// Result code: 0 = Success, others = Failed
    /// Full list: https://developers.momo.vn/#/docs/aiov2/?id=m%c3%a3-l%e1%bb%97i-result-code
    /// </summary>
    [JsonPropertyName("resultCode")]
    public int ResultCode { get; set; }

    /// <summary>
    /// Result message from Momo
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; } = default!;

    /// <summary>
    /// Payment type
    /// </summary>
    [JsonPropertyName("payType")]
    public string PayType { get; set; } = default!;

    /// <summary>
    /// Timestamp from Momo
    /// </summary>
    [JsonPropertyName("responseTime")]
    public long ResponseTime { get; set; }

    /// <summary>
    /// Extra data (base64 encoded if provided)
    /// </summary>
    [JsonPropertyName("extraData")]
    public string ExtraData { get; set; } = default!;

    /// <summary>
    /// HMAC SHA256 signature to verify authenticity
    /// </summary>
    [JsonPropertyName("signature")]
    public string Signature { get; set; } = default!;
}

/// <summary>
/// Momo Return URL Query Parameters (when user is redirected back)
/// Same structure as IPN but via GET request
/// </summary>
public class MomoReturnUrlQuery
{
    public string PartnerCode { get; set; } = default!;
    public string OrderId { get; set; } = default!;
    public string RequestId { get; set; } = default!;
    public long Amount { get; set; }
    public string OrderInfo { get; set; } = default!;
    public string OrderType { get; set; } = default!;
    public long TransId { get; set; }
    public int ResultCode { get; set; }
    public string Message { get; set; } = default!;
    public string PayType { get; set; } = default!;
    public long ResponseTime { get; set; }
    public string ExtraData { get; set; } = default!;
    public string Signature { get; set; } = default!;
}
