using Catalog.Application.Models.Filters;
using Catalog.Application.Models.Results;
using Catalog.Domain.Entities;
using Marten;
using Marten.Pagination;

namespace Catalog.Application.Features.Category.Queries;

public sealed record GetAllCategoriesQuery(GetAllCategoriesFilter filter, PaginationRequest pagination)
    : IQuery<GetAllCategoriesResult>;

public sealed class GetAllCategoriesQueryHandler(IDocumentSession session, IMapper mapper)
    : IQueryHandler<GetAllCategoriesQuery, GetAllCategoriesResult>
{
    public async Task<GetAllCategoriesResult> Handle(GetAllCategoriesQuery query, CancellationToken cancellationToken)
    {
        var filter = query.filter;
        var paging = query.pagination;
        var categoryQuery = session.Query<CategoryEntity>().AsQueryable();

        if (filter.ParentId.HasValue)
        {
            categoryQuery = categoryQuery.Where(x => x.ParentId == filter.ParentId.Value);
        }

        var categories = await categoryQuery
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

        var items = mapper.Map<List<Catalog.Application.Dtos.Categories.CategoryDto>>(categories);

        // Enrich with parent names
        if (items.Count > 0)
        {
            var allCategories = await session.Query<CategoryEntity>()
                .ToListAsync(cancellationToken);

            foreach (var item in items)
            {
                if (item.ParentId.HasValue)
                {
                    var parent = allCategories.FirstOrDefault(c => c.Id == item.ParentId.Value);
                    if (parent != null)
                    {
                        item.ParentName = parent.Name;
                    }
                }
            }
        }

        return new GetAllCategoriesResult(items);
    }
}
