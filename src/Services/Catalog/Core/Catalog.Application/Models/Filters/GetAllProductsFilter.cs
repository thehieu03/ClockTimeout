namespace Catalog.Application.Models.Filters;

public record class GetAllProductsFilter(
    string? SearchText,
    Guid[]? Ids,
    Guid? BrandId = null,
    Guid[]? CategoryIds = null,
    bool? Published = null,
    bool? Featured = null);