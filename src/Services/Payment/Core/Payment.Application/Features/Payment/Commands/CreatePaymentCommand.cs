using BuildingBlocks.CQRS;
using Common.ValueObjects;
using Payment.Application.Dtos;
using Payment.Domain.Enums;

namespace Payment.Application.Features.Payment.Commands;

public record CreatePaymentCommand(
    Guid OrderId,
    decimal Amount,
    PaymentMethod Method,
    Actor Actor
) : ICommand<PaymentDto>;
