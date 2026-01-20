using Catalog.Application.Models.Filters;
using Marten.Pagination;

namespace Catalog.Application.Features.Product.Queries;

public sealed record GetProductsQuery(GetProductsFilter filter, PaginationRequest pagination) : IQuery<Dtos.Products.GetProductsResult>;

public sealed record GetAllProductsQuery(GetProductsFilter filter, PaginationRequest pagination) : IQuery<Dtos.Products.GetProductsResult>;

public sealed class GetAllProductsQueryHandler(IDocumentSession session, IMapper mapper) : IQueryHandler<GetAllProductsQuery, Dtos.Products.GetProductsResult>
{
    public async Task<Dtos.Products.GetProductsResult> Handle(GetAllProductsQuery query, CancellationToken cancellationToken)
    {
        var filter = query.filter;
        var paging = query.pagination;
        var productQuery = session.Query<ProductEntity>().AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchText))
        {
            var search = filter.SearchText.Trim();
            productQuery = productQuery.Where(x => x.Name != null && x.Name.Contains(search));
        }

        if (filter.Ids?.Length > 0)
        {
            productQuery = productQuery.Where(x => filter.Ids.Contains(x.Id));
        }

        var totalCount = await productQuery.CountAsync(cancellationToken);
        var pagedResult = await productQuery
            .OrderByDescending(x => x.CreatedOnUtc)
            .ToPagedListAsync(paging.PageNumber, paging.PageSize, cancellationToken);

        var products = pagedResult.ToList();
        var items = mapper.Map<List<Dtos.Products.ProductDto>>(products);

        if (items.Count > 0)
        {
            var categories = await session.Query<CategoryEntity>()
                .ToListAsync(cancellationToken);
            var brands = await session.Query<BrandEntity>()
                .ToListAsync(cancellationToken);

            foreach (var item in items)
            {
                var product = products.FirstOrDefault(x => x.Id == item.Id);
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
                            item.CategoryIds ??= [];
                            item.CategoryIds.Add(category.Id);
                        }
                    }
                }

                if (product.BrandId.HasValue)
                {
                    var brand = brands.FirstOrDefault(b => b.Id == product.BrandId.Value);
                    if (brand != null)
                    {
                        item.BrandName = brand.Name;
                        item.BrandId = brand.Id;
                    }
                }
            }
        }

        return new Dtos.Products.GetProductsResult(items, totalCount, paging);
    }
}

public sealed class GetProductsQueryHandler(IDocumentSession session, IMapper mapper) : IQueryHandler<GetProductsQuery, Dtos.Products.GetProductsResult>
{
    public async Task<Dtos.Products.GetProductsResult> Handle(GetProductsQuery query, CancellationToken cancellationToken)
    {
        var filter = query.filter;
        var paging = query.pagination;
        var productQuery = session.Query<ProductEntity>().AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchText))
        {
            var search = filter.SearchText.Trim();
            productQuery = productQuery.Where(x => x.Name != null && x.Name.Contains(search));
        }

        if (filter.Ids?.Length > 0)
        {
            productQuery = productQuery.Where(x => filter.Ids.Contains(x.Id));
        }

        var totalCount = await productQuery.CountAsync(cancellationToken);
        var pagedResult = await productQuery
            .OrderByDescending(x => x.CreatedOnUtc)
            .ToPagedListAsync(paging.PageNumber, paging.PageSize, cancellationToken);

        var products = pagedResult.ToList();
        var items = mapper.Map<List<Dtos.Products.ProductDto>>(products);

        if (items.Count > 0)
        {
            var categories = await session.Query<CategoryEntity>()
                .ToListAsync(cancellationToken);
            var brands = await session.Query<BrandEntity>()
                .ToListAsync(cancellationToken);

            foreach (var item in items)
            {
                var product = products.FirstOrDefault(x => x.Id == item.Id);
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
                            item.CategoryIds ??= [];
                            item.CategoryIds.Add(category.Id);
                        }
                    }
                }

                if (product.BrandId.HasValue)
                {
                    var brand = brands.FirstOrDefault(b => b.Id == product.BrandId.Value);
                    if (brand != null)
                    {
                        item.BrandName = brand.Name;
                        item.BrandId = brand.Id;
                    }
                }
            }
        }

        return new Dtos.Products.GetProductsResult(items, totalCount, paging);
    }
}
