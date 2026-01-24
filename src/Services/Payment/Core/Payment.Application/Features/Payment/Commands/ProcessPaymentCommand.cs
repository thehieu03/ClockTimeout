using BuildingBlocks.CQRS;
using Common.ValueObjects;
using Payment.Application.Models.Results;

namespace Payment.Application.Features.Payment.Commands;

public record ProcessPaymentCommand(
    Guid PaymentId,
    string? ReturnUrl,
    string? CancelUrl,
    Actor Actor
) : ICommand<ProcessPaymentResult>;
