using BuildingBlocks.CQRS;
using Common.ValueObjects;

namespace Payment.Application.Features.Payment.Commands;

public record RefundPaymentResult(bool IsSuccess, string Message);

public record RefundPaymentCommand(
    Guid PaymentId,
    string Reason,
    Actor Actor
) : ICommand<RefundPaymentResult>;
