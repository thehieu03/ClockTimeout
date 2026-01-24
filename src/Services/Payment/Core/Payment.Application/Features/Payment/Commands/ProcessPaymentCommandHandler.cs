using AutoMapper;
using BuildingBlocks.CQRS;
using BuildingBlocks.Extensions;
using Common.Constants;
using Microsoft.Extensions.Logging;
using Payment.Application.Dtos;
using Payment.Application.Gateways;
using Payment.Application.Gateways.Models;
using Payment.Application.Models.Results;
using Payment.Domain.Abstractions;
using Payment.Domain.Enums;
using Payment.Domain.Repositories;

namespace Payment.Application.Features.Payment.Commands;

public class ProcessPaymentCommandHandler(
    IPaymentRepository paymentRepository,
    IUnitOfWork unitOfWork,
    IPaymentGatewayFactory gatewayFactory,
    IMapper mapper,
    ILogger<ProcessPaymentCommandHandler> logger)
    : ICommandHandler<ProcessPaymentCommand, ProcessPaymentResult>
{
    public async Task<ProcessPaymentResult> Handle(
        ProcessPaymentCommand command,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Processing payment: {PaymentId}", command.PaymentId);

        // 1. Get payment from database
        var payment = await paymentRepository.GetByIdAsync(command.PaymentId, cancellationToken);

        if (payment is null)
        {
            throw new NotFoundException(MessageCode.PaymentNotFound, command.PaymentId);
        }

        // 2. Validate payment status
        if (payment.Status == PaymentStatus.Completed)
        {
            logger.LogWarning("Payment {PaymentId} is already completed", command.PaymentId);
            return ProcessPaymentResult.Success(mapper.Map<PaymentDto>(payment));
        }

        if (payment.Status != PaymentStatus.Pending)
        {
            throw new InvalidOperationException($"Cannot process payment in {payment.Status} status");
        }

        // 3. Mark as processing
        payment.MarkAsProcessing(command.Actor.Value);
        paymentRepository.Update(payment);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        try
        {
            // 4. Get appropriate gateway
            var gateway = gatewayFactory.GetGateway(payment.Method);

            // 5. Build gateway request
            var gatewayRequest = new PaymentGatewayRequest
            {
                PaymentId = payment.Id,
                OrderId = payment.OrderId,
                Amount = payment.Amount,
                Method = payment.Method,
                Description = $"Payment for Order {payment.OrderId}",
                ReturnUrl = command.ReturnUrl,
                CancelUrl = command.CancelUrl
            };

            // 6. Process payment through gateway
            logger.LogInformation(
                "Calling payment gateway {Method} for payment {PaymentId}",
                payment.Method,
                payment.Id);

            var gatewayResult = await gateway.ProcessPaymentAsync(gatewayRequest, cancellationToken);

            // 7. Handle result
            if (gatewayResult.IsSuccess)
            {
                payment.Complete(gatewayResult.TransactionId!, gatewayResult.RawResponse, command.Actor.Value);

                logger.LogInformation(
                    "Payment {PaymentId} completed successfully. TransactionId: {TransactionId}",
                    payment.Id,
                    gatewayResult.TransactionId);
            }
            else
            {
                payment.MarkAsFailed(
                    gatewayResult.ErrorCode ?? "UNKNOWN",
                    gatewayResult.ErrorMessage ?? "Unknown error",
                    gatewayResult.RawResponse,
                    command.Actor.Value);

                logger.LogWarning(
                    "Payment {PaymentId} failed. Error: {ErrorCode} - {ErrorMessage}",
                    payment.Id,
                    gatewayResult.ErrorCode,
                    gatewayResult.ErrorMessage);
            }

            // 8. Save changes and dispatch domain events
            paymentRepository.Update(payment);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            // 9. Return result
            var paymentDto = mapper.Map<PaymentDto>(payment);

            return gatewayResult.IsSuccess
                ? ProcessPaymentResult.Success(paymentDto, gatewayResult.RedirectUrl)
                : ProcessPaymentResult.Failure(paymentDto, gatewayResult.ErrorMessage!);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing payment {PaymentId}", payment.Id);

            // Mark as failed
            payment.MarkAsFailed("SYSTEM_ERROR", ex.Message, null, command.Actor.Value);
            paymentRepository.Update(payment);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            var paymentDto = mapper.Map<PaymentDto>(payment);
            return ProcessPaymentResult.Failure(paymentDto, ex.Message);
        }
    }
}
