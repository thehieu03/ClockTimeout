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
    string? LastModifiedBy) : IDomainEvent, INotification;