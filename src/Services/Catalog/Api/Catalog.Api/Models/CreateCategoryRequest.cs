namespace Catalog.Api.Models;

public sealed class CreateCategoryRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public Guid? ParentId { get; set; }
}
