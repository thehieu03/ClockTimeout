using Catalog.Api.Constants;
using Catalog.Application.Features.Brand.Queries;
using Catalog.Application.Models.Results;
using Common.Models.Reponses;

namespace Catalog.Api.Endpoints;

public sealed class GetAllBrands : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet(ApiRoutes.Brand.GetAll, HandleGetAllBrandsAsync)
            .Produces<ApiGetResponse<GetAllBrandsResult>>(StatusCodes.Status200OK)
            .WithTags(ApiRoutes.Brand.Tags)
            .WithName(nameof(GetAllBrands))
            .Produces(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status400BadRequest);
        // .RequireAuthorization();
    }

    private async Task<ApiGetResponse<GetAllBrandsResult>> HandleGetAllBrandsAsync(
        ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new GetAllBrandsQuery();
        var result = await sender.Send(query, cancellationToken);
        return new ApiGetResponse<GetAllBrandsResult>(result);
    }
}
