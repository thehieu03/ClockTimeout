namespace EventSourcing;

public record IntegrationEvent
{
    public string Id { get; init; } = default!;

    public DateTimeOffset OccurredOn { get; init; } = DateTimeOffset.UtcNow;

    public string? EventType => GetType()?.AssemblyQualifiedName;
}