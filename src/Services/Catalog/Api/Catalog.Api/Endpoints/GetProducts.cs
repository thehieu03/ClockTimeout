using Catalog.Api.Constants;
using Catalog.Application.Dtos.Products;
using Catalog.Application.Features.Product.Queries;
using Catalog.Application.Models.Filters;
using Common.Models;
using Common.Models.Reponses;

namespace Catalog.Api.Endpoints;

public sealed class GetProducts : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet(ApiRoutes.Product.GetProducts, HandleGetProductsAsync)
            .Produces<ApiGetResponse<GetProductsResult>>(StatusCodes.Status200OK)
            .WithTags(ApiRoutes.Product.Tags)
            .WithName(nameof(GetProducts))
            .Produces(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .RequireAuthorization();
    }

    private async Task<ApiGetResponse<GetProductsResult>> HandleGetProductsAsync(
        ISender sender,
        [AsParameters] GetProductsFilter filter,
        [AsParameters] PaginationRequest pagination,
        CancellationToken cancellationToken)
    {
        var query = new GetAllProductsQuery(filter, pagination);
        var result = await sender.Send(query, cancellationToken);
        return new ApiGetResponse<GetProductsResult>(result);
    }
}
