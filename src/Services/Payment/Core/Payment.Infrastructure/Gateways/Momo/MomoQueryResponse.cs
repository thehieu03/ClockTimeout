

namespace Payment.Infrastructure.Gateways.Momo;

public class MomoQueryResponse
{
    [JsonPropertyName("partnerCode")]
    public string PartnerCode { get; set; } = default!;

    [JsonPropertyName("orderId")]
    public string OrderId { get; set; } = default!;

    [JsonPropertyName("requestId")]
    public string RequestId { get; set; } = default!;

    [JsonPropertyName("extraData")]
    public string ExtraData { get; set; } = default!;

    [JsonPropertyName("amount")]
    public long Amount { get; set; }

    [JsonPropertyName("transId")]
    public long TransId { get; set; }

    [JsonPropertyName("payType")]
    public string PayType { get; set; } = default!;

    [JsonPropertyName("resultCode")]
    public int ResultCode { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = default!;
}
