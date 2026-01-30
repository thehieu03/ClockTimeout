using FluentValidation;

namespace Payment.Application.Features.Payment.Commands;

public class HandlePaymentCallbackCommandValidator : AbstractValidator<HandlePaymentCallbackCommand>
{
    public HandlePaymentCallbackCommandValidator()
    {
        RuleFor(x => x.PaymentId)
            .NotEmpty()
            .WithMessage("PaymentId is required");

        RuleFor(x => x.ResultCode)
            .NotEmpty()
            .WithMessage("ResultCode is required");

        RuleFor(x => x.ResultMessage)
            .NotEmpty()
            .MaximumLength(500)
            .WithMessage("ResultMessage is required and must be <= 500 characters");

        RuleFor(x => x.RawResponse)
            .NotEmpty()
            .WithMessage("RawResponse is required for audit");

        RuleFor(x => x.Gateway)
            .NotEmpty()
            .Must(g => g == "Momo" || g == "VnPay")
            .WithMessage("Gateway must be either 'Momo' or 'VnPay'");

        // TransactionId is required only when IsSuccess = true
        When(x => x.IsSuccess, () =>
        {
            RuleFor(x => x.TransactionId)
                .NotEmpty()
                .WithMessage("TransactionId is required for successful payments");
        });
    }
}
