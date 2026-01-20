namespace Catalog.Application.Models.Filters;

public record class GetAllCategoriesFilter(
    string? SearchText,
    Guid? ParentId);
