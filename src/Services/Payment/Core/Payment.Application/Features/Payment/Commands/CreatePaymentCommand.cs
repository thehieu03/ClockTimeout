using BuildingBlocks.CQRS;
using Common.Constants;
using FluentValidation;
using Payment.Application.Dtos;
using Payment.Domain.Abstractions;
using Payment.Domain.Entities;

namespace Payment.Application.Features.Payment.Commands;

public record CreatePaymentCommand(CreatePaymentDto Dto, string? PerformedBy = null) : ICommand<Guid>;

public class CreatePaymentCommandValidator : AbstractValidator<CreatePaymentCommand>
{
    public CreatePaymentCommandValidator()
    {
        RuleFor(x => x.Dto)
            .NotNull()
            .WithMessage(MessageCode.BadRequest)
            .DependentRules(() =>
            {
                RuleFor(x => x.Dto.OrderId)
                    .NotEmpty()
                    .WithMessage(MessageCode.OrderIdIsRequired);

                RuleFor(x => x.Dto.Amount)
                    .GreaterThan(0)
                    .WithMessage(MessageCode.PriceMustBeGreaterThanZero);

                RuleFor(x => x.Dto.Method)
                    .IsInEnum()
                    .WithMessage(MessageCode.BadRequest);
            });
    }
}

public class CreatePaymentCommandHandler(IUnitOfWork unitOfWork) : ICommandHandler<CreatePaymentCommand, Guid>
{
    public async Task<Guid> Handle(CreatePaymentCommand command, CancellationToken cancellationToken)
    {
        var dto = command.Dto;

        var payment = PaymentEntity.Create(
            orderId: dto.OrderId,
            amount: dto.Amount,
            method: dto.Method,
            createdBy: command.PerformedBy
        );

        await unitOfWork.Payments.AddAsync(payment, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return payment.Id;
    }
}
