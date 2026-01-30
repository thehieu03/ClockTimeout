namespace Payment.Infrastructure.Gateways.VnPay.Models;

/// <summary>
/// VNPay IPN Request Model
/// Docs: https://sandbox.vnpayment.vn/apis/docs/huong-dan-tich-hop/#code-ipn-url
/// </summary>
public class VnPayIpnRequest
{
    /// <summary>
    /// Transaction reference from your system
    /// </summary>
    public string vnp_TxnRef { get; set; } = default!;

    /// <summary>
    /// Payment amount (* 100, no decimal)
    /// </summary>
    public long vnp_Amount { get; set; }

    /// <summary>
    /// Order info
    /// </summary>
    public string vnp_OrderInfo { get; set; } = default!;

    /// <summary>
    /// Response code: 00 = Success
    /// </summary>
    public string vnp_ResponseCode { get; set; } = default!;

    /// <summary>
    /// VNPay transaction number
    /// </summary>
    public string vnp_TransactionNo { get; set; } = default!;

    /// <summary>
    /// Bank code
    /// </summary>
    public string vnp_BankCode { get; set; } = default!;

    /// <summary>
    /// Payment date (yyyyMMddHHmmss)
    /// </summary>
    public string vnp_PayDate { get; set; } = default!;

    /// <summary>
    /// TMN Code (VNPay merchant code)
    /// </summary>
    public string vnp_TmnCode { get; set; } = default!;

    /// <summary>
    /// Transaction status: 00 = Success
    /// </summary>
    public string vnp_TransactionStatus { get; set; } = default!;

    /// <summary>
    /// Secure hash to verify
    /// </summary>
    public string vnp_SecureHash { get; set; } = default!;

    public string? vnp_CardType { get; set; }
    public string? vnp_BankTranNo { get; set; }
}
