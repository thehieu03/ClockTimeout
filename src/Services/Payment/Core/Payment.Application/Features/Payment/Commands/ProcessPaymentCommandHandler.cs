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
using Polly;
using Polly.Retry;

namespace Payment.Application.Features.Payment.Commands;

public partial class ProcessPaymentCommandHandler(
    IPaymentRepository paymentRepository,
    IUnitOfWork unitOfWork,
    IPaymentGatewayFactory gatewayFactory,
    IMapper mapper,
    ILogger<ProcessPaymentCommandHandler> logger)
    : ICommandHandler<ProcessPaymentCommand, ProcessPaymentResult>
{
    // Retry configuration
    private const int MaxRetryAttempts = 3;
    private static readonly TimeSpan BaseDelay = TimeSpan.FromSeconds(1);

    public async Task<ProcessPaymentResult> Handle(
        ProcessPaymentCommand command,
        CancellationToken cancellationToken)
    {
        LogProcessingPayment(logger, command.PaymentId);

        // 1. Get payment from database
        var payment = await paymentRepository.GetByIdAsync(command.PaymentId, cancellationToken);

        if (payment is null)
        {
            throw new NotFoundException(MessageCode.PaymentNotFound, command.PaymentId);
        }

        // 2. Validate payment status (Idempotency check)
        if (payment.Status == PaymentStatus.Completed)
        {
            LogPaymentAlreadyCompleted(logger, command.PaymentId);
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

            // 6. Build Retry Policy with Exponential Backoff
            var retryPipeline = new ResiliencePipelineBuilder()
                .AddRetry(new RetryStrategyOptions
                {
                    MaxRetryAttempts = MaxRetryAttempts,
                    Delay = BaseDelay,
                    BackoffType = DelayBackoffType.Exponential,
                    OnRetry = args =>
                    {
                        LogRetryAttempt(logger, args.AttemptNumber, payment.Id, args.Outcome.Exception?.Message ?? "Unknown", args.RetryDelay.TotalSeconds);
                        return ValueTask.CompletedTask;
                    }
                })
                .Build();

            // 7. Process payment through gateway WITH RETRY
            LogCallingGateway(logger, payment.Method, payment.Id);

            var gatewayResult = await retryPipeline.ExecuteAsync(
                async ct => await gateway.ProcessPaymentAsync(gatewayRequest, ct), 
                cancellationToken);

            // 7. Handle result
            if (gatewayResult.IsSuccess)
            {
                // Check if this is a redirect-based payment (VnPay, Momo, etc.)
                // If there's a redirect URL, keep the payment in Processing status
                // The payment will be completed via callback (IPN/Callback endpoint)
                if (!string.IsNullOrEmpty(gatewayResult.RedirectUrl))
                {
                    // Store the transaction ID for later callback verification
                    payment.SetTransactionId(gatewayResult.TransactionId!);
                    
                    LogPaymentRequiresRedirect(logger, payment.Id, gatewayResult.TransactionId!, gatewayResult.RedirectUrl);
                }
                else
                {
                    // Non-redirect payment (e.g., COD, direct card charge)
                    // Complete the payment immediately
                    payment.Complete(gatewayResult.TransactionId!, gatewayResult.RawResponse, command.Actor.Value);

                    LogPaymentCompleted(logger, payment.Id, gatewayResult.TransactionId!);
                }
            }
            else
            {
                payment.MarkAsFailed(
                    gatewayResult.ErrorCode ?? "UNKNOWN",
                    gatewayResult.ErrorMessage ?? "Unknown error",
                    gatewayResult.RawResponse,
                    command.Actor.Value);

                LogPaymentFailed(logger, payment.Id, gatewayResult.ErrorCode ?? "UNKNOWN", gatewayResult.ErrorMessage ?? "Unknown error");
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

    // LoggerMessage source generators for high-performance logging
    [LoggerMessage(Level = LogLevel.Information, Message = "Processing payment: {PaymentId}")]
    private static partial void LogProcessingPayment(ILogger logger, Guid paymentId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Payment {PaymentId} is already completed")]
    private static partial void LogPaymentAlreadyCompleted(ILogger logger, Guid paymentId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Retry attempt {AttemptNumber} for payment {PaymentId} due to: {ErrorMessage}. Waiting {WaitSeconds}s...")]
    private static partial void LogRetryAttempt(ILogger logger, int attemptNumber, Guid paymentId, string errorMessage, double waitSeconds);

    [LoggerMessage(Level = LogLevel.Information, Message = "Calling payment gateway {Method} for payment {PaymentId}")]
    private static partial void LogCallingGateway(ILogger logger, PaymentMethod method, Guid paymentId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Payment {PaymentId} requires redirect. TransactionId: {TransactionId}, RedirectUrl: {RedirectUrl}")]
    private static partial void LogPaymentRequiresRedirect(ILogger logger, Guid paymentId, string transactionId, string redirectUrl);

    [LoggerMessage(Level = LogLevel.Information, Message = "Payment {PaymentId} completed successfully. TransactionId: {TransactionId}")]
    private static partial void LogPaymentCompleted(ILogger logger, Guid paymentId, string transactionId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Payment {PaymentId} failed. Error: {ErrorCode} - {ErrorMessage}")]
    private static partial void LogPaymentFailed(ILogger logger, Guid paymentId, string errorCode, string errorMessage);
}
