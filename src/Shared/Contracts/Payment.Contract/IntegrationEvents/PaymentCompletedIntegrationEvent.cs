namespace Payment.Contract.IntegrationEvents;

/// <summary>
/// Integration event published when a payment is completed successfully
/// </summary>
public record PaymentCompletedIntegrationEvent(
    Guid Id,
    Guid PaymentId,
    Guid OrderId,
    string TransactionId,
    decimal Amount,
    DateTimeOffset OccurredOn
);
