using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Payment.Contract.IntegrationEvents;
using Payment.Domain.Entities;
using Payment.Domain.Events;
using Payment.Infrastructure.Data;

namespace Payment.Infrastructure.EventHandlers;

/// <summary>
/// Handles PaymentFailedDomainEvent by creating an integration event and saving it to the outbox
/// </summary>
public class PaymentFailedDomainEventHandler(
    ApplicationDbContext dbContext,
    ILogger<PaymentFailedDomainEventHandler> logger)
    : INotificationHandler<PaymentFailedDomainEvent>
{
    public async Task Handle(PaymentFailedDomainEvent @event, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Handling PaymentFailedDomainEvent for PaymentId: {PaymentId}, OrderId: {OrderId}, Error: {ErrorMessage}",
            @event.PaymentId, @event.OrderId, @event.ErrorMessage);

        // 1. Create Integration Event
        var integrationEvent = new PaymentFailedIntegrationEvent(
            Id: Guid.NewGuid(),
            PaymentId: @event.PaymentId,
            OrderId: @event.OrderId,
            ErrorCode: @event.ErrorCode,
            ErrorMessage: @event.ErrorMessage,
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

        logger.LogInformation(
            "Created outbox message {OutboxMessageId} for PaymentFailedIntegrationEvent",
            outboxMessage.Id);
    }
}
