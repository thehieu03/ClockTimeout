namespace Catalog.Application.Models.Results;

public sealed class GetAllCategoriesResult
{
    #region Fields, Properties and Indexers

    public List<Catalog.Application.Dtos.Categories.CategoryDto> Items { get; init; }

    #endregion

    #region Ctors

    public GetAllCategoriesResult(List<Catalog.Application.Dtos.Categories.CategoryDto> items)
    {
        Items = items;
    }

    #endregion
}
