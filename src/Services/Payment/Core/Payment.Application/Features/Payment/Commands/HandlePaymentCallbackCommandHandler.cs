using BuildingBlocks.CQRS;
using BuildingBlocks.Extensions;
using Microsoft.Extensions.Logging;
using Payment.Domain.Abstractions;
using Payment.Domain.Enums;
using Payment.Domain.Repositories;

namespace Payment.Application.Features.Payment.Commands;

public class HandlePaymentCallbackCommandHandler(
    IPaymentRepository paymentRepository,
    IUnitOfWork unitOfWork,
    ILogger<HandlePaymentCallbackCommandHandler> logger)
    : ICommandHandler<HandlePaymentCallbackCommand, HandlePaymentCallbackResult>
{
    public async Task<HandlePaymentCallbackResult> Handle(
        HandlePaymentCallbackCommand command,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Processing payment callback. PaymentId: {PaymentId}, Gateway: {Gateway}, Success: {Success}, ResultCode: {Code}",
            command.PaymentId, command.Gateway, command.IsSuccess, command.ResultCode);

        // 1. Load payment from database
        var payment = await paymentRepository.GetByIdAsync(command.PaymentId, cancellationToken);
        if (payment == null)
        {
            logger.LogError("Payment {PaymentId} not found", command.PaymentId);
            throw new NotFoundException("Payment", command.PaymentId);
        }

        // 2. Idempotency check - if already completed, don't process again
        if (payment.Status == PaymentStatus.Completed)
        {
            logger.LogInformation(
                "Payment {PaymentId} already completed. Skipping duplicate callback.",
                command.PaymentId);

            return new HandlePaymentCallbackResult(
                Success: true,
                Message: "Payment already completed",
                WasAlreadyProcessed: true
            );
        }

        // 3. Check if payment is in valid state for callback
        if (payment.Status != PaymentStatus.Pending && payment.Status != PaymentStatus.Processing)
        {
            logger.LogWarning(
                "Payment {PaymentId} in unexpected status {Status} for callback",
                command.PaymentId, payment.Status);

            return new HandlePaymentCallbackResult(
                Success: false,
                Message: $"Payment in invalid status: {payment.Status}",
                WasAlreadyProcessed: false
            );
        }

        // 4. Update payment based on callback result
        try
        {
            if (command.IsSuccess)
            {
                // Success case
                payment.Complete(
                    transactionId: command.TransactionId!,
                    gatewayResponse: command.RawResponse);

                logger.LogInformation(
                    "Payment {PaymentId} marked as Completed. TransactionId: {TransactionId}",
                    command.PaymentId, command.TransactionId);
            }
            else
            {
                // Failure case
                payment.MarkAsFailed(
                    errorCode: command.ResultCode,
                    errorMessage: command.ResultMessage,
                    gatewayResponse: command.RawResponse);

                logger.LogWarning(
                    "Payment {PaymentId} marked as Failed. ErrorCode: {ErrorCode}, Message: {Message}",
                    command.PaymentId, command.ResultCode, command.ResultMessage);
            }

            // 5. Save changes (this will also publish domain events via UnitOfWork)
            paymentRepository.Update(payment);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Payment {PaymentId} callback processed successfully",
                command.PaymentId);

            return new HandlePaymentCallbackResult(
                Success: true,
                Message: "Callback processed successfully",
                WasAlreadyProcessed: false
            );
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error processing payment {PaymentId} callback",
                command.PaymentId);

            throw; // Let global exception handler deal with it
        }
    }
}
