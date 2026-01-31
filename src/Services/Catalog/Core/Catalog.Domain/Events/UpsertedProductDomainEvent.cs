using Catalog.Domain.Abstractions;
using Catalog.Domain.Enums;
using MediatR;

namespace Catalog.Domain.Events;

public sealed record UpsertedProductDomainEvent(
    Guid Id,
    string Name,
    string Sku,
    string Slug,
    decimal Price,
    decimal? SalePrice,
    List<string>? Categories,
    List<string>? Images,
    string Thumbnail,
    ProductStatus Status,
    DateTimeOffset CreatedOnUtc,
    string CreatedBy,
    DateTimeOffset? LastModifiedOnUtc,
    string? LastModifiedBy) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
    public string EventType => GetType().AssemblyQualifiedName ?? string.Empty;
}