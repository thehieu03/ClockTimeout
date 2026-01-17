namespace Catalog.Application.Models.Filters;

public record class GetPublishProductsFilter(
    string? SearchText,
    Guid? BrandId = null,
    Guid[]? CategoryIds = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    bool? Featured = null);