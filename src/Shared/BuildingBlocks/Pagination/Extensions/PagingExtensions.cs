using Common.Models;
using MongoDB.Driver;

namespace BuildingBlocks.Pagination.Extensions;

public static class PagingExtensions
{

    #region Methods

    public static IFindFluent<TDocument, TProjection> WithPaging<TDocument, TProjection>(
        this IFindFluent<TDocument, TProjection> fluent, PaginationRequest paging)
    {
        if (fluent is null) throw new ArgumentException(nameof(fluent));
        if (paging is null) throw new ArgumentException(nameof(paging));
        var (pageNumber, pageSize, skip) = Normalize(paging);
        return fluent.Skip(skip).Limit(pageSize);
        return null;
    }
    public static IQueryable<T> WithPaging<T>(
        this IQueryable<T> query, PaginationRequest paging
    )
    {
        if(query is null) throw new ArgumentException(nameof(query));
        if(paging is null) throw new ArgumentException(nameof(paging));
        var (pageNumber, pageSize, skip) = Normalize(paging);
        return query.Skip(skip).Take(pageSize);
    }
    public static int GetTotalPage(this PaginationRequest paging, long totalCount)
    {
        if(paging.PageSize <= 0) throw new ArgumentException(nameof(paging.PageSize),"Page size must be greater than 0");
        return (int)Math.Ceiling(totalCount / (double)paging.PageSize);
    }
    private static (int pageNumber, int pageSize, int skip) Normalize(PaginationRequest paging)
    {
        var pageNumber = paging.PageNumber <= 0 ? 1 : paging.PageNumber;
        var pageSize = paging.PageSize <= 0 ? 10 : paging.PageSize;
        checked
        {
            var skip = (pageNumber - 1) * pageSize;
            return (pageNumber, pageSize, skip);
        }
    }

    #endregion
}