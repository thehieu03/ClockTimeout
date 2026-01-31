using BuildingBlocks.CQRS;
using BuildingBlocks.Extensions;
using Payment.Application.Gateways;
using Payment.Domain.Abstractions;
using Payment.Domain.Enums;
using Payment.Domain.Repositories;

namespace Payment.Application.Features.Payment.Commands;

public class RefundPaymentCommandHandler(
    IPaymentRepository paymentRepository,
    IPaymentGatewayFactory gatewayFactory,
    // Note: IUnitOfWork might need to be specific or injected differently depending on project setup. 
    // The previous file used 'IUnitOfWork unitOfWork' but inside it accessed 'unitOfWork.Payments'.
    // Checking the ProcessPaymentCommandHandler, it used 'IUnitOfWork unitOfWork'.
    // I will stick to what was asked in the prompt, but might need adjustment if IUnitOfWork is different.
    IUnitOfWork unitOfWork)
    : ICommandHandler<RefundPaymentCommand, RefundPaymentResult>
{
    public async Task<RefundPaymentResult> Handle(RefundPaymentCommand command, CancellationToken cancellationToken)
    {
        // 1. Load Payment
        var payment = await paymentRepository.GetByIdAsync(command.PaymentId, cancellationToken);
        if (payment == null) throw new NotFoundException("Payment", command.PaymentId);

        // 2. Validate State
        if (payment.Status != PaymentStatus.Completed)
        {
            return new RefundPaymentResult(false, $"Cannot refund payment in status {payment.Status}");
        }

        // 3. Call Gateway
        // We need to catch exceptions here? The prompt didn't show it but it's good practice.
        // Prompt code is simple, I follows prompt code.
        var gateway = gatewayFactory.GetGateway(payment.Method);
        var refundResult = await gateway.RefundPaymentAsync(payment.TransactionId!, payment.Amount, cancellationToken);

        if (!refundResult.IsSuccess)
        {
            return new RefundPaymentResult(false, $"Gateway Refund Failed: {refundResult.ErrorMessage}");
        }

        // 4. Update DB
        payment.Refund(command.Reason, refundResult.TransactionId, command.Actor.Value); 
        paymentRepository.Update(payment);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new RefundPaymentResult(true, "Refund Successful");
    }
}
