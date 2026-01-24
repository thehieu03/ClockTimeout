namespace Payment.Application.Models.Results;

public class VnPayCallbackResult
{
    public bool IsValid { get; init; }
    public bool IsSuccess { get; init; }
    public string? TransactionId { get; init; }
    public string? VnPayTransactionNo { get; init; }
    public string? ResponseCode { get; init; }
    public string? TransactionStatus { get; init; }
    public decimal Amount { get; init; }
    public string? BankCode { get; init; }
    public string? BankTransactionNo { get; init; }
    public string? PayDate { get; init; }
    public string? Message { get; init; }
    public string? RawData { get; init; }
    public static VnPayCallbackResult Invalid(string message) => new VnPayCallbackResult { IsValid = false, Message = message };
    public static VnPayCallbackResult FromVnPayResponse(Dictionary<string, string> vnpParams, bool isValidSignature)
    {
        if (!isValidSignature)
        {
            return Invalid("Invalid signature");
        }
        var responseCode = vnpParams.GetValueOrDefault("vnp_ResponseCode", "");
        var transactionStatus = vnpParams.GetValueOrDefault("vnp_TransactionStatus", "");
        var amountStr = vnpParams.GetValueOrDefault("vnp_Amount", "0");
        return new VnPayCallbackResult
        {
            IsValid = true,
            IsSuccess = responseCode == "00" && transactionStatus == "00",
            TransactionId = vnpParams.GetValueOrDefault("vnp_TxnRef"),
            VnPayTransactionNo = vnpParams.GetValueOrDefault("vnp_TransactionNo"),
            ResponseCode = responseCode,
            TransactionStatus = transactionStatus,
            Amount = decimal.Parse(amountStr) / 100, // Convert from smallest unit
            BankCode = vnpParams.GetValueOrDefault("vnp_BankCode"),
            BankTransactionNo = vnpParams.GetValueOrDefault("vnp_BankTranNo"),
            PayDate = vnpParams.GetValueOrDefault("vnp_PayDate"),
            RawData = string.Join("&", vnpParams.Select(x => $"{x.Key}={x.Value}"))
        };

    }
}
