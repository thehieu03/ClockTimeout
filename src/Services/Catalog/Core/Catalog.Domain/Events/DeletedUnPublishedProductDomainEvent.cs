namespace Catalog.Domain.Events;

public sealed record DeletedUnPublishedProductDomainEvent(Guid ProductId):IDomainEvent;