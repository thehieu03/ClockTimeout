using Common.Constants;
using FluentValidation;

namespace Payment.Application.Features.Payment.Commands;

public class CreatePaymentCommandValidator : AbstractValidator<CreatePaymentCommand>
{
    public CreatePaymentCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage(MessageCode.OrderIdIsRequired);

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage(MessageCode.AmountMustBeGreaterThanZero);

        RuleFor(x => x.Method)
            .IsInEnum()
            .WithMessage(MessageCode.PaymentMethodInvalid);

        RuleFor(x => x.Actor)
            .NotNull()
            .WithMessage(MessageCode.ActorIsRequired);
    }
}
