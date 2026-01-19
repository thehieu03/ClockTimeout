namespace Catalog.Domain.Entities;

public sealed class ProductEntity : Aggregate<Guid>
{
    #region Fields, Properties and Indexers

    public string? Name { get; set; }
    public string? Sku { get; set; }
    public string? ShortDescription { get; set; }
    public string? LongDescription { get; set; }
    public string? Slug { get; set; }
    public string? Barcode { get; set; }
    public decimal Price { get; set; }
    public decimal? SalePrice { get; set; }
    public List<Guid>? CategoryIds { get; set; }
    public List<ProductImageEntity>? Images { get; set; }
    public ProductImageEntity? Thumbnail { get; set; }
    public List<string>? Colors { get; set; }
    public List<string>? Sizes { get; set; }
    public List<string>? Tags { get; set; }
    public bool Published { get; set; }
    public bool Featured { get; set; }
    public ProductStatus Status { get; set; }
    public Guid? BrandId { get; set; }
    public string? SEOTitle { get; set; }
    public string? SEODescription { get; set; }
    public string? Unit { get; set; }
    public decimal? Weight { get; set; }
    // created producdt command handler
    public static ProductEntity Create(
        Guid id,
        string name,
        string sku,
        string shortDescription,
        string longDescription,
        string slug,
        decimal price,
        decimal? salePrice,
        List<Guid>? categoryIds,
        Guid? brandId,
        string performedBy)
    {
        return new ProductEntity
        {
            Id = id,
            Name = name,
            Sku = sku,
            ShortDescription = shortDescription,
            LongDescription = longDescription,
            Slug = slug,
            Price = price,
            SalePrice = salePrice,
            Status = ProductStatus.OutOfStock,
            Published = false,
            CategoryIds = categoryIds,
            BrandId = brandId,
            CreatedBy = performedBy,
            LastModifiedBy = performedBy,
            CreatedOnUtc = DateTimeOffset.UtcNow,
            LastModifiedOnUtc = DateTimeOffset.UtcNow
        };
    }
    // update product command handler
    public void Update(
    string name,
    string sku,
    string shortDescription,
    string longDescription,
    decimal price,
    decimal? salePrice,
    string performedBy)
    {
        Name = name;
        Sku = sku;
        ShortDescription = shortDescription;
        LongDescription = longDescription;
        Price = price;
        SalePrice = salePrice;
        LastModifiedBy = performedBy;
        LastModifiedOnUtc = DateTimeOffset.UtcNow;
    }
    // publish product command handler
    public void Publish(string performedBy)
    {
        Published = true;
        LastModifiedBy = performedBy;
        LastModifiedOnUtc = DateTimeOffset.UtcNow;
    }
    // unpublish product command handler

    public void Unpublish(string performedBy)
    {
        Published = false;
        LastModifiedBy = performedBy;
        LastModifiedOnUtc = DateTimeOffset.UtcNow;
    }
    // update colors command handler
    public void UpdateColors(List<string>? colors, string performedBy)
    {
        Colors = colors;
        LastModifiedBy = performedBy;
        LastModifiedOnUtc = DateTimeOffset.UtcNow;
    }
    // update sizes command handler
    public void UpdateSizes(List<string>? sizes, string performedBy)
    {
        Sizes = sizes;
        LastModifiedBy = performedBy;
        LastModifiedOnUtc = DateTimeOffset.UtcNow;
    }
    // update tags command handler
    public void UpdateTags(List<string>? tags, string performedBy)
    {
        Tags = tags;
        LastModifiedBy = performedBy;
        LastModifiedOnUtc = DateTimeOffset.UtcNow;
    }
    // update SEO command handler
    public void UpdateSEO(string? seoTitle, string? seoDescription, string performedBy)
    {
        SEOTitle = seoTitle;
        SEODescription = seoDescription;
        LastModifiedBy = performedBy;
        LastModifiedOnUtc = DateTimeOffset.UtcNow;
    }
    // update featured command handler
    public void UpdateFeatured(bool featured, string performedBy)
    {
        Featured = featured;
        LastModifiedBy = performedBy;
        LastModifiedOnUtc = DateTimeOffset.UtcNow;
    }
    // update barcode command handler
    public void UpdateBarcode(string? barcode, string performedBy)
    {
        Barcode = barcode;
        LastModifiedBy = performedBy;
        LastModifiedOnUtc = DateTimeOffset.UtcNow;
    }
    // update unit and weight command handler
    public void UpdateUnitAndWeight(string? unit, decimal? weight, string performedBy)
    {
        Unit = unit;
        Weight = weight;
        LastModifiedBy = performedBy;
        LastModifiedOnUtc = DateTimeOffset.UtcNow;
    }
    #endregion
}


