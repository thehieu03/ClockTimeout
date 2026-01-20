namespace Catalog.Application.Models.Results;

public sealed class GetAllBrandsResult
{
    #region Fields, Properties and Indexers

    public List<Catalog.Application.Dtos.Brands.BrandDto> Items { get; init; }

    #endregion

    #region Ctors

    public GetAllBrandsResult(List<Catalog.Application.Dtos.Brands.BrandDto> items)
    {
        Items = items;
    }

    #endregion
}
