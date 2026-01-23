using BuildingBlocks.CQRS;
using BuildingBlocks.Extensions;
using Common.Constants;
using FluentValidation;
using Payment.Domain.Abstractions;

namespace Payment.Application.Features.Payment.Commands;

public record CompletePaymentCommand(Guid PaymentId, string TransactionId, string? PerformedBy = null) : ICommand;

public class CompletePaymentCommandValidator : AbstractValidator<CompletePaymentCommand>
{
    public CompletePaymentCommandValidator()
    {
        RuleFor(x => x.PaymentId)
            .NotEmpty()
            .WithMessage(MessageCode.IdIsRequired);

        RuleFor(x => x.TransactionId)
            .NotEmpty()
            .WithMessage("TRANSACTION_ID_IS_REQUIRED");
    }
}

public class CompletePaymentCommandHandler(IUnitOfWork unitOfWork) : ICommandHandler<CompletePaymentCommand>
{
    public async Task<MediatR.Unit> Handle(CompletePaymentCommand command, CancellationToken cancellationToken)
    {
        var payment = await unitOfWork.Payments.GetByIdAsync(command.PaymentId, cancellationToken);

        if (payment is null)
            throw new NotFoundException(MessageCode.NotFound, command.PaymentId);

        payment.Complete(command.TransactionId, command.PerformedBy);

        unitOfWork.Payments.Update(payment);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return MediatR.Unit.Value;
    }
}
