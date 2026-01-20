namespace Catalog.Application.Models.Results;

public sealed class GetTreeCategoriesResult
{
    #region Fields, Properties and Indexers

    public List<Catalog.Application.Dtos.Categories.CategoryTreeItemDto> Items { get; init; }

    #endregion

    #region Ctors

    public GetTreeCategoriesResult(List<Catalog.Application.Dtos.Categories.CategoryTreeItemDto> items)
    {
        Items = items;
    }

    #endregion
}
