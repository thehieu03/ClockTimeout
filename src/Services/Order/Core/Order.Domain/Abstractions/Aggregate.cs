namespace Order.Domain.Abstractions;

public abstract class Aggregate<TId>:Entity<TId>,IAggregate<TId>
{

    #region Fields, Properties and Indexers

    private readonly List<IDomainEvent> _domainEvents = new();  
    #endregion

    #region Implementations

    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    public IDomainEvent[] ClearDomainEvents()
    {
        IDomainEvent[] dequeuedEvents=_domainEvents.ToArray();
        _domainEvents.Clear();
        return dequeuedEvents;
    }

    #endregion

    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
 
}