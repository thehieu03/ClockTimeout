namespace Payment.Contract.IntegrationEvents;

/// <summary>
/// Integration event published when a payment fails
/// </summary>
public record PaymentFailedIntegrationEvent(
    Guid Id,
    Guid PaymentId,
    Guid OrderId,
    string ErrorCode,
    string ErrorMessage,
    DateTimeOffset OccurredOn
);
