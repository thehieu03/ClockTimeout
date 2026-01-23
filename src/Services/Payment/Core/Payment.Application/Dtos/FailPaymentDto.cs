namespace Payment.Application.Dtos;

public sealed class FailPaymentDto
{
    public string ErrorMessage { get; set; } = default!;
}
