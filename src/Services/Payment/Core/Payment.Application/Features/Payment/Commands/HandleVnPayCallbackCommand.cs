using BuildingBlocks.CQRS;
using BuildingBlocks.Extensions;
using Common.Constants;
using FluentValidation;
using Payment.Application.Models.Results;
using Payment.Domain.Abstractions;

namespace Payment.Application.Features.Payment.Commands;

public record HandleVnPayCallbackCommand(VnPayCallbackResult CallbackResult) : ICommand<HandleVnPayCallbackResult>;

public class HandleVnPayCallbackCommandValidator : AbstractValidator<HandleVnPayCallbackCommand>
{
    public HandleVnPayCallbackCommandValidator()
    {
        RuleFor(x => x.CallbackResult)
            .NotNull()
            .WithMessage("Callback result is required");

        RuleFor(x => x.CallbackResult.TransactionId)
            .NotEmpty()
            .When(x => x.CallbackResult != null)
            .WithMessage("Transaction ID is required");
    }
}

public class HandleVnPayCallbackCommandHandler(IUnitOfWork unitOfWork) 
    : ICommandHandler<HandleVnPayCallbackCommand, HandleVnPayCallbackResult>
{
    public async Task<HandleVnPayCallbackResult> Handle(
        HandleVnPayCallbackCommand command, 
        CancellationToken cancellationToken)
    {
        var callbackResult = command.CallbackResult;

        // Validate callback result
        if (!callbackResult.IsValid)
        {
            return HandleVnPayCallbackResult.Failure(callbackResult.Message ?? "Invalid callback");
        }

        // Find payment by transaction reference (TxnRef)
        var payment = await unitOfWork.Payments.GetByTransactionIdAsync(
            callbackResult.TransactionId!, 
            cancellationToken);

        if (payment is null)
        {
            return HandleVnPayCallbackResult.Failure($"Payment not found for TxnRef: {callbackResult.TransactionId}");
        }

        // Process based on callback result
        if (callbackResult.IsSuccess)
        {
            // Complete the payment
            payment.Complete(
                callbackResult.VnPayTransactionNo ?? callbackResult.TransactionId!, 
                callbackResult.RawData,
                "VNPay");
        }
        else
        {
            // Fail the payment
            payment.MarkAsFailed(
                callbackResult.ResponseCode ?? "UNKNOWN",
                callbackResult.Message ?? $"Payment failed with code: {callbackResult.ResponseCode}",
                callbackResult.RawData,
                "VNPay");
        }

        unitOfWork.Payments.Update(payment);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return HandleVnPayCallbackResult.Success(payment.Id);
    }
}
