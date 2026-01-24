namespace Payment.Application.Gateways.Models;

public record PaymentGatewayResult
{
    public bool IsSuccess { get; init; }
    public string? TransactionId { get; init; }
    public string? ErrorCode { get; init; }
    public string? ErrorMessage { get; init; }
    public string? RedirectUrl { get; init; }
    public string? RawResponse { get; init; }
    public Dictionary<string, string> Metadata { get; init; } = new();

    public static PaymentGatewayResult Success(string transactionId, string? redirectUrl = null, string? rawResponse = null)
        => new()
        {
            IsSuccess = true,
            TransactionId = transactionId,
            RedirectUrl = redirectUrl,
            RawResponse = rawResponse
        };

    public static PaymentGatewayResult Failure(string errorCode, string errorMessage, string? rawResponse = null)
        => new()
        {
            IsSuccess = false,
            ErrorCode = errorCode,
            ErrorMessage = errorMessage,
            RawResponse = rawResponse
        };
}
