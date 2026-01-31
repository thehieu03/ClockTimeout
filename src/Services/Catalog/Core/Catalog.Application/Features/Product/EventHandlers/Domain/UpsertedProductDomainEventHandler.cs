using Catalog.Domain.Events;
using Marten;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Catalog.Application.Features.Product.EventHandlers.Domain;

public sealed class UpsertedProductDomainEventHandler(ILogger<UpsertedProductDomainEventHandler> logger) 
    : INotificationHandler<UpsertedProductDomainEvent>
{

    #region Implementations

    public async Task Handle(UpsertedProductDomainEvent @event, CancellationToken cancellationToken)
    {
        logger.LogInformation("Domain Event Handler: {DomainEvent}", @event.GetType().Name);
        await PushToOutboxAsync(@event, cancellationToken);
    }

    #endregion

    #region Methods

    private async Task PushToOutboxAsync(UpsertedProductDomainEvent @event, CancellationToken cancellationToken)
    {
        
    }

    #endregion
}