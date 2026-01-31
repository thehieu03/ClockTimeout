#region using

using Catalog.Domain.Abstractions;

#endregion

namespace Catalog.Domain.Events;

public sealed record DeletedUnPublishedProductDomainEvent(Guid ProductId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
    public string EventType => GetType().AssemblyQualifiedName ?? string.Empty;
}