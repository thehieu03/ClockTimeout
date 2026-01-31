using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Payment.Contract.IntegrationEvents;
using Payment.Domain.Entities;
using Payment.Domain.Events;
using Payment.Infrastructure.Data;

namespace Payment.Infrastructure.EventHandlers;

/// <summary>
/// Handles PaymentCompletedDomainEvent by creating an integration event and saving it to the outbox
/// </summary>
public partial class PaymentCompletedDomainEventHandler(
    ApplicationDbContext dbContext,
    ILogger<PaymentCompletedDomainEventHandler> logger)
    : INotificationHandler<PaymentCompletedDomainEvent>
{
    public async Task Handle(PaymentCompletedDomainEvent @event, CancellationToken cancellationToken)
    {
        LogHandlingPaymentCompleted(logger, @event.PaymentId, @event.OrderId);

        // 1. Create Integration Event
        var integrationEvent = new PaymentCompletedIntegrationEvent(
            Id: Guid.NewGuid(),
            PaymentId: @event.PaymentId,
            OrderId: @event.OrderId,
            TransactionId: @event.TransactionId,
            Amount: @event.Amount,
            OccurredOn: @event.OccurredOn
        );

        // 2. Create Outbox Message
        var outboxMessage = OutboxMessage.Create(
            id: Guid.NewGuid(),
            eventType: integrationEvent.GetType().AssemblyQualifiedName!,
            content: JsonConvert.SerializeObject(integrationEvent, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            }),
            occurredOnUtc: DateTimeOffset.UtcNow
        );

        // 3. Add to DbContext (will be saved with the same transaction as the command)
        await dbContext.OutboxMessages.AddAsync(outboxMessage, cancellationToken);

        LogOutboxMessageCreated(logger, outboxMessage.Id);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Handling PaymentCompletedDomainEvent for PaymentId: {PaymentId}, OrderId: {OrderId}")]
    private static partial void LogHandlingPaymentCompleted(ILogger logger, Guid paymentId, Guid orderId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Created outbox message {OutboxMessageId} for PaymentCompletedIntegrationEvent")]
    private static partial void LogOutboxMessageCreated(ILogger logger, Guid outboxMessageId);
}
