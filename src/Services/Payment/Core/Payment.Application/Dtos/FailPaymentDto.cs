namespace Payment.Application.Dtos;

public sealed class FailPaymentDto
{
    public string ErrorCode { get; set; } = default!;
    public string ErrorMessage { get; set; } = default!;
}
