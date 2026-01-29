namespace Payment.Infrastructure.Gateways.Momo;

public class MomoQueryRequest
{
    [JsonPropertyName("partnerCode")]
    public string PartnerCode { get; set; } = default!;

    [JsonPropertyName("requestId")]
    public string RequestId { get; set; } = default!;

    [JsonPropertyName("orderId")]
    public string OrderId { get; set; } = default!;

    [JsonPropertyName("signature")]
    public string Signature { get; set; } = default!;

    [JsonPropertyName("lang")]
    public string Lang { get; set; } = "vi";
}
