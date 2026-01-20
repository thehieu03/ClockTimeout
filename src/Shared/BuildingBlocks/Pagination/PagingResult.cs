using BuildingBlocks.Pagination.Extensions;
using Common.Models;

namespace BuildingBlocks.Pagination;

public sealed class PagingResult
{

    #region Fields, Properties and Indexers

    public long TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public bool HasItem { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }

    #endregion

    #region ctor

    private PagingResult(long totalCount, PaginationRequest pagination)
    {
        TotalCount = totalCount;
        PageNumber = pagination.PageNumber;
        PageSize = pagination.PageSize;
        HasItem = totalCount > 0;
    }

    #endregion

    #region Methods

    public static PagingResult Of(long totalCount, PaginationRequest pagination)
    {
        var result = new PagingResult(totalCount, pagination);
        if (pagination.PageSize > 0)
        {
            result.TotalPages = pagination.GetTotalPage(totalCount);
            result.HasNextPage = pagination.PageNumber < result.TotalPages;
            result.HasPreviousPage = pagination.PageNumber > 1 && pagination.PageNumber <= result.TotalPages;
        }
        return result;
    }

    #endregion

}