using BuildingBlocks.CQRS;
using BuildingBlocks.Extensions;
using Common.Constants;
using FluentValidation;
using Payment.Domain.Abstractions;

namespace Payment.Application.Features.Payment.Commands;

public record RefundPaymentCommand(Guid PaymentId, string? RefundReason, string? RefundTransactionId = null, string? PerformedBy = null) : ICommand;

public class RefundPaymentCommandValidator : AbstractValidator<RefundPaymentCommand>
{
    public RefundPaymentCommandValidator()
    {
        RuleFor(x => x.PaymentId)
            .NotEmpty()
            .WithMessage(MessageCode.IdIsRequired);

        RuleFor(x => x.RefundReason)
            .MaximumLength(500)
            .WithMessage(MessageCode.Max500Characters);
    }
}

public class RefundPaymentCommandHandler(IUnitOfWork unitOfWork) : ICommandHandler<RefundPaymentCommand>
{
    public async Task<MediatR.Unit> Handle(RefundPaymentCommand command, CancellationToken cancellationToken)
    {
        var payment = await unitOfWork.Payments.GetByIdAsync(command.PaymentId, cancellationToken);

        if (payment is null)
            throw new NotFoundException(MessageCode.NotFound, command.PaymentId);

        payment.Refund(command.RefundReason, command.RefundTransactionId, command.PerformedBy);

        unitOfWork.Payments.Update(payment);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return MediatR.Unit.Value;
    }
}
