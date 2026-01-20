using BuildingBlocks.CQRS;
using BuildingBlocks.Extensions;
using Catalog.Application.Dtos.Products;
using Catalog.Application.Mappings;
using Catalog.Application.Models.Results;
using Catalog.Domain.Entities;
using Common.Constants;
using Marten;

namespace Catalog.Application.Features.Product.Queries;

public sealed record GetProductByIdQuery(Guid ProductId) : IQuery<GetProductByIdResult>;

public sealed class GetProductByIdQueryHandler(IDocumentSession session)
    : IQueryHandler<GetProductByIdQuery, GetProductByIdResult>
{

    #region Implementations

    public async Task<GetProductByIdResult> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        // Load product entity
        var product = await session.LoadAsync<ProductEntity>(request.ProductId, cancellationToken) ??
                      throw new NotFoundException(MessageCode.ResourceNotFound, request.ProductId);

        // Load related categories and brands
        var categories = await session.Query<CategoryEntity>()
            .ToListAsync(cancellationToken);
        var brands = await session.Query<BrandEntity>()
            .ToListAsync(cancellationToken);

        // Map product entity to DTO
        var response = CatalogMapper.Mapper.Map<Dtos.Products.ProductDto>(product);

        // Map category names
        if (product.CategoryIds != null && product.CategoryIds.Count > 0)
        {
            response.CategoryNames ??= [];
            foreach (var categoryId in product.CategoryIds)
            {
                var category = categories.FirstOrDefault(x => x.Id == categoryId);
                if (category != null && !string.IsNullOrWhiteSpace(category.Name))
                {
                    response.CategoryNames.Add(category.Name);
                }
            }
        }

        // Map brand name
        if (product.BrandId.HasValue)
        {
            var brand = brands.FirstOrDefault(x => x.Id == product.BrandId.Value);
            if (brand != null && !string.IsNullOrWhiteSpace(brand.Name))
            {
                response.BrandName = brand.Name;
            }
        }

        return new GetProductByIdResult(response);
    }

    #endregion

}