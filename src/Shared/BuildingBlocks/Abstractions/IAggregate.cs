namespace BuildingBlocks.Abstractions;

public interface IAggregate<T> : ICreationAuditable, IModificationAuditable
{
    IReadOnlyList<IDomainEvent> DomainEvents { get; }
    IDomainEvent[] ClearDomainEvents();
}
