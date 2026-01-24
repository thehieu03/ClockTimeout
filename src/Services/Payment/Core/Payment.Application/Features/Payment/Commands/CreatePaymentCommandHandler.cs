using AutoMapper;
using BuildingBlocks.CQRS;
using Microsoft.Extensions.Logging;
using Payment.Application.Dtos;
using Payment.Domain.Entities;
using Payment.Domain.Repositories;
using Payment.Domain.Abstractions;

namespace Payment.Application.Features.Payment.Commands;

public class CreatePaymentCommandHandler(
    IPaymentRepository paymentRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<CreatePaymentCommandHandler> logger)
    : ICommandHandler<CreatePaymentCommand, PaymentDto>
{
    public async Task<PaymentDto> Handle(CreatePaymentCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Creating payment for OrderId: {OrderId}, Amount: {Amount}, Method: {Method}",
            command.OrderId,
            command.Amount,
            command.Method);

        // 1. Check if payment already exists for this order
        var existingPayment = await paymentRepository.GetByOrderIdAsync(command.OrderId, cancellationToken);
        if (existingPayment != null && existingPayment.Status == Domain.Enums.PaymentStatus.Pending)
        {
            logger.LogWarning("Payment already exists for OrderId: {OrderId}", command.OrderId);
            return mapper.Map<PaymentDto>(existingPayment);
        }

        // 2. Create new Payment entity
        var payment = PaymentEntity.Create(
            orderId: command.OrderId,
            amount: command.Amount,
            method: command.Method,
            createdBy: command.Actor.Value
        );

        // 3. Save to database
        await paymentRepository.AddAsync(payment, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Payment created successfully. PaymentId: {PaymentId}, OrderId: {OrderId}",
            payment.Id,
            command.OrderId);

        // 5. Map and return
        return mapper.Map<PaymentDto>(payment);
    }
}
