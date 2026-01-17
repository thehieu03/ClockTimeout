using BuildingBlocks.Abstractions;
using Catalog.Domain.Enums;

namespace Catalog.Domain.Entities;

public sealed class ProductEntity : Aggregate<Guid>
{
    public string? Name { get; set; }
    public string? Sku { get; set; }
    public string? ShortDescriptions { get; set; }
    public string? LongDescriptions { get; set; }
    public decimal Price { get; set; }
    public decimal? SalePrice { get; set; }
    public ProductStatus Status { get; set; }
    public static ProductEntity Create(Guid id, string name, string sku, string shortDescription, string longDescription, decimal price, decimal? salePrice, string performedBy)
    {
        return new ProductEntity
        {
            Id = id,
            Name = name,
            Sku = sku,
            ShortDescriptions = shortDescription,
            LongDescriptions = longDescription,
            Price = price,
            SalePrice = salePrice,
            Status = ProductStatus.OutOfStock,
            CreatedBy = performedBy,
            LastModifiedBy = performedBy,
            CreatedOnUtc = DateTimeOffset.UtcNow,
            LastModifiedOnUtc = DateTimeOffset.UtcNow
        };
    }
}


