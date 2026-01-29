namespace Payment.Infrastructure.Gateways.Momo.Models;

public class MomoIpnRequest
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
