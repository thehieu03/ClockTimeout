using Catalog.Api.Constants;
using Catalog.Application.Features.Category.Queries;
using Catalog.Application.Models.Results;
using Common.Models.Reponses;

namespace Catalog.Api.Endpoints;

public sealed class GetTreeCategories : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet(ApiRoutes.Category.GetTree, HandleGetTreeCategoriesAsync)
            .Produces<ApiGetResponse<GetTreeCategoriesResult>>(StatusCodes.Status200OK)
            .WithTags(ApiRoutes.Category.Tags)
            .WithName(nameof(GetTreeCategories))
            .Produces(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .RequireAuthorization();
    }

    private async Task<ApiGetResponse<GetTreeCategoriesResult>> HandleGetTreeCategoriesAsync(
        ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new GetTreeCategoriesQuery();
        var result = await sender.Send(query, cancellationToken);
        return new ApiGetResponse<GetTreeCategoriesResult>(result);
    }
}
