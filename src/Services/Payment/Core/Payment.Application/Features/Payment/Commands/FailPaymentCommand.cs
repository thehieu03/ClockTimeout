using BuildingBlocks.CQRS;
using BuildingBlocks.Extensions;
using Common.Constants;
using FluentValidation;
using Payment.Domain.Abstractions;

namespace Payment.Application.Features.Payment.Commands;

public record FailPaymentCommand(
    Guid PaymentId,
    string ErrorCode,
    string ErrorMessage,
    string? PerformedBy = null) : ICommand;

public class FailPaymentCommandValidator : AbstractValidator<FailPaymentCommand>
{
    public FailPaymentCommandValidator()
    {
        RuleFor(x => x.PaymentId)
            .NotEmpty()
            .WithMessage(MessageCode.IdIsRequired);

        RuleFor(x => x.ErrorCode)
            .NotEmpty()
            .WithMessage("ERROR_CODE_IS_REQUIRED")
            .MaximumLength(100)
            .WithMessage(MessageCode.Max100Characters);

        RuleFor(x => x.ErrorMessage)
            .NotEmpty()
            .WithMessage(MessageCode.ErrorMessageIsRequired)
            .MaximumLength(500)
            .WithMessage(MessageCode.Max500Characters);
    }
}

public class FailPaymentCommandHandler(IUnitOfWork unitOfWork) : ICommandHandler<FailPaymentCommand>
{
    public async Task<MediatR.Unit> Handle(FailPaymentCommand command, CancellationToken cancellationToken)
    {
        var payment = await unitOfWork.Payments.GetByIdAsync(command.PaymentId, cancellationToken);

        if (payment is null)
            throw new NotFoundException(MessageCode.NotFound, command.PaymentId);

        payment.MarkAsFailed(command.ErrorCode, command.ErrorMessage, null, command.PerformedBy);

        unitOfWork.Payments.Update(payment);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return MediatR.Unit.Value;
    }
}
