using Catalog.Api.Constants;
using Catalog.Application.Features.Category.Queries;
using Catalog.Application.Models.Filters;
using Catalog.Application.Models.Results;
using Common.Models;
using Common.Models.Reponses;

namespace Catalog.Api.Endpoints;

public sealed class GetAllCategories : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet(ApiRoutes.Category.GetAll, HandleGetAllCategoriesAsync)
            .Produces<ApiGetResponse<GetAllCategoriesResult>>(StatusCodes.Status200OK)
            .WithTags(ApiRoutes.Category.Tags)
            .WithName(nameof(GetAllCategories))
            .Produces(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .RequireAuthorization();
    }

    private async Task<ApiGetResponse<GetAllCategoriesResult>> HandleGetAllCategoriesAsync(
        ISender sender,
        [AsParameters] GetAllCategoriesFilter filter,
        [AsParameters] PaginationRequest pagination,
        CancellationToken cancellationToken)
    {
        var query = new GetAllCategoriesQuery(filter, pagination);
        var result = await sender.Send(query, cancellationToken);
        return new ApiGetResponse<GetAllCategoriesResult>(result);
    }
}
