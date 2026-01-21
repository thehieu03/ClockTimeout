using MediatR;

namespace Order.Domain.Abstractions;

public class IDomainEvent:INotification
{

    #region Fields, Properties and Indexers
    Guid EventId { get; set; }=Guid.NewGuid();
    public DateTimeOffset OccurredOnUtc { get; set; } = DateTime.Now;
    public string EventType => GetType()?.AssemblyQualifiedName??string.Empty;
    
    

    #endregion
}