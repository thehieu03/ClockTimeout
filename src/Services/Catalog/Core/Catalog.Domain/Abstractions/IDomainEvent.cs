using MediatR;

namespace Catalog.Domain.Abstractions;

public interface IDomainEvent : INotification
{
    Guid EventId { get; }
    DateTimeOffset OccurredOn { get; }
    string EventType { get; }
}