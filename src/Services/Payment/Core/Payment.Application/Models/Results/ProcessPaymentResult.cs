using Payment.Application.Dtos;

namespace Payment.Application.Models.Results;

public record ProcessPaymentResult
{
    public bool IsSuccess { get; init; }
    public PaymentDto Payment { get; init; } = null!;
    public string? RedirectUrl { get; init; }
    public string? ErrorMessage { get; init; }

    public static ProcessPaymentResult Success(PaymentDto payment, string? redirectUrl = null)
        => new() { IsSuccess = true, Payment = payment, RedirectUrl = redirectUrl };

    public static ProcessPaymentResult Failure(PaymentDto payment, string errorMessage)
        => new() { IsSuccess = false, Payment = payment, ErrorMessage = errorMessage };
}
