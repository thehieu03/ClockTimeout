using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BuildingBlocks.Abstractions;

public interface IDomainEvent

{
    Guid EventId => Guid.NewGuid();
    DateTimeOffset OccurredOn => DateTimeOffset.UtcNow;
    string EventType => GetType()?.AssemblyQualifiedName ?? string.Empty;
}
