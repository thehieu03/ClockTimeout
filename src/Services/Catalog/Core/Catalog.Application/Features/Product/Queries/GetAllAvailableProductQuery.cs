using Catalog.Application.Models.Filters;
using Catalog.Domain.Enums;

namespace Catalog.Application.Features.Product.Queries;

public sealed record GetAllAvailableProductQuery(GetAllProductsFilter Filter) : IQuery<GetAllAvailableProductsResult>;
public sealed class GetAllAvailableProductsQueryHandler(IDocumentSession session, IMapper mapper) : IRequestHandler<GetAllAvailableProductQuery, GetAllAvailableProductsResult>
{
    public async Task<GetAllAvailableProductsResult> Handle(GetAllAvailableProductQuery query, CancellationToken cancellationToken)
    {
        var filter = query.Filter;
        var productQuery = session.Query<ProductEntity>()
            .Where(x => x.Published && x.Status == ProductStatus.InStock);
        if (!string.IsNullOrWhiteSpace(filter.SearchText))
        {
            var search = filter.SearchText.Trim();
            productQuery = productQuery.Where(x => x.Name != null && x.Name.Contains(search));
        }
        if (filter.Ids?.Length > 0)
        {
            productQuery = productQuery.Where(x => filter.Ids.Contains(x.Id));
        }
        var result = await productQuery
            .OrderByDescending(x => x.CreatedOnUtc)
            .ToListAsync(cancellationToken);
        var products = result.ToList();
        var items = mapper.Map<List<Dtos.Products.ProductDto>>(products);
        if (items.Count > 0)
        {
            var categories = await session.Query<CategoryEntity>()
            .ToListAsync(cancellationToken);
            var brands = await session.Query<BrandEntity>()
                .ToListAsync(cancellationToken);

            foreach (var item in items)
            {
                var product = result.FirstOrDefault(p => p.Id == item.Id);

                if (product == null) continue;

                if (product.CategoryIds != null && product.CategoryIds.Count > 0)
                {
                    foreach (var categoryId in product.CategoryIds)
                    {
                        var category = categories.FirstOrDefault(c => c.Id == categoryId);
                        if (category != null)
                        {
                            item.CategoryNames ??= [];
                            item.CategoryNames.Add(category.Name!);
                        }
                    }
                }

                if (product.BrandId.HasValue)
                {
                    var brand = brands.FirstOrDefault(b => b.Id == product.BrandId.Value);
                    if (brand != null)
                    {
                        item.BrandName = brand.Name;
                    }
                }
            }
        }
        return new GetAllAvailableProductsResult(items);
    }
}
