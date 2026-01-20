using BuildingBlocks.CQRS;
using Catalog.Application.Models.Results;

namespace Catalog.Application.Features.Product.Queries;

public sealed record GetProductByIdQuery(Guid ProductId):IQuery<GetProductByIdResult>
{

}