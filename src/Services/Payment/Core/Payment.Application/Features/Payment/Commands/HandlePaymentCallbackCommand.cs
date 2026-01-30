using BuildingBlocks.CQRS;
using Common.ValueObjects;

namespace Payment.Application.Features.Payment.Commands;

/// <summary>
/// Command to handle payment callback from gateway (IPN)
/// This command is idempotent - can be called multiple times safely
/// </summary>
public record HandlePaymentCallbackCommand(
    /// <summary>
    /// Payment ID in our system
    /// </summary>
    Guid PaymentId,

    /// <summary>
    /// Whether payment was successful according to gateway
    /// </summary>
    bool IsSuccess,

    /// <summary>
    /// Transaction ID from gateway (for completed payments)
    /// </summary>
    string? TransactionId,

    /// <summary>
    /// Result/Error code from gateway
    /// </summary>
    string ResultCode,

    /// <summary>
    /// Message from gateway
    /// </summary>
    string ResultMessage,

    /// <summary>
    /// Raw JSON response from gateway (for audit)
    /// </summary>
    string RawResponse,

    /// <summary>
    /// Gateway name ("Momo", "VnPay")
    /// </summary>
    string Gateway,

    /// <summary>
    /// Actor performing this action (System for IPN)
    /// </summary>
    Actor Actor
) : ICommand<HandlePaymentCallbackResult>;

/// <summary>
/// Result of callback handling
/// </summary>
public record HandlePaymentCallbackResult(
    bool Success,
    string Message,
    bool WasAlreadyProcessed
);
