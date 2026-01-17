namespace Catalog.Application.Models.Filters;

public record class GetProductsFilter(
    string? SearchText,
    Guid[]? Ids,
    Guid? BrandId = null,
    Guid[]? CategoryIds = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    bool? Published = null,
    bool? Featured = null);