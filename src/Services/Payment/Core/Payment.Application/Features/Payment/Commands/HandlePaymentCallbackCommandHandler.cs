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
    : ICommandHandler<HandlePaymentCallbackCommand, bool>
{
    public async Task<bool> Handle(HandlePaymentCallbackCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling Payment Callback. Id: {PaymentId}, Success: {IsSuccess}", command.PaymentId, command.IsSuccess);

        var payment = await paymentRepository.GetByIdAsync(command.PaymentId, cancellationToken);
        if (payment == null)
        {
            throw new NotFoundException("Payment", command.PaymentId);
        }

        // Idempotency check simple (Status check)
        if (payment.Status == PaymentStatus.Completed)
        {
            logger.LogInformation("Payment {PaymentId} already completed.", command.PaymentId);
            return true;
        }

        if (command.IsSuccess)
        {
            payment.Complete(command.TransactionId, command.RawResponse);
        }
        else
        {
            payment.MarkAsFailed("GATEWAY_FAILED", "Gateway reported failure", command.RawResponse);
        }

        paymentRepository.Update(payment);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
