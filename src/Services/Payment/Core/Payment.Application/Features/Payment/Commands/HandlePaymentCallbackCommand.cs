using BuildingBlocks.CQRS;
using Common.ValueObjects;

namespace Payment.Application.Features.Payment.Commands;

public record HandlePaymentCallbackCommand(
    Guid PaymentId,
    bool IsSuccess,
    string TransactionId,
    string RawResponse,
    Actor Actor
) : ICommand<bool>;
