namespace EventSourcing.Catalog;

public sealed record class DeletedUnPublishedProductIntegrationEvent 
{
    public Guid ProductId { get; init; }
}