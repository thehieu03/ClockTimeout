using Catalog.Application.Models.Results;
using Catalog.Domain.Entities;
using Marten;

namespace Catalog.Application.Features.Category.Queries;

public sealed record GetTreeCategoriesQuery() : IQuery<GetTreeCategoriesResult>;

public sealed class GetTreeCategoriesQueryHandler(IDocumentSession session, IMapper mapper)
    : IQueryHandler<GetTreeCategoriesQuery, GetTreeCategoriesResult>
{
    public async Task<GetTreeCategoriesResult> Handle(GetTreeCategoriesQuery query, CancellationToken cancellationToken)
    {
        var allCategories = await session.Query<CategoryEntity>()
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

        var categoryDtos = mapper.Map<List<Catalog.Application.Dtos.Categories.CategoryTreeItemDto>>(allCategories);

        // Build tree structure
        var rootCategories = categoryDtos.Where(c => c.ParentId == null).ToList();
        var categoryLookup = categoryDtos.ToDictionary(c => c.Id);

        foreach (var category in categoryDtos)
        {
            if (category.ParentId.HasValue && categoryLookup.TryGetValue(category.ParentId.Value, out var parent))
            {
                parent.Children ??= new List<Catalog.Application.Dtos.Categories.CategoryTreeItemDto>();
                parent.Children.Add(category);
            }
        }

        return new GetTreeCategoriesResult(rootCategories);
    }
}
