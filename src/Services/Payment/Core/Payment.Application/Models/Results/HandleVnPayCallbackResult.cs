namespace Payment.Application.Models.Results;

public class HandleVnPayCallbackResult
{
    public bool IsSuccess { get; init; }
    public string? Message { get; init; }
    public Guid? PaymentId { get; init; }

    public static HandleVnPayCallbackResult Success(Guid paymentId) => new()
    {
        IsSuccess = true,
        PaymentId = paymentId,
        Message = "Payment processed successfully"
    };

    public static HandleVnPayCallbackResult Failure(string message) => new()
    {
        IsSuccess = false,
        Message = message
    };
}
