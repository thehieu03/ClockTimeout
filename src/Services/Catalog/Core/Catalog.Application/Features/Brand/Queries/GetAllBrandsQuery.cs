using Catalog.Application.Models.Results;
using Catalog.Domain.Entities;
using Marten;

namespace Catalog.Application.Features.Brand.Queries;

public sealed record GetAllBrandsQuery() : IQuery<GetAllBrandsResult>;

public sealed class GetAllBrandsQueryHandler(IDocumentSession session, IMapper mapper)
    : IQueryHandler<GetAllBrandsQuery, GetAllBrandsResult>
{
    public async Task<GetAllBrandsResult> Handle(GetAllBrandsQuery query, CancellationToken cancellationToken)
    {
        var brands = await session.Query<BrandEntity>()
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

        var items = mapper.Map<List<Catalog.Application.Dtos.Brands.BrandDto>>(brands);

        return new GetAllBrandsResult(items);
    }
}
