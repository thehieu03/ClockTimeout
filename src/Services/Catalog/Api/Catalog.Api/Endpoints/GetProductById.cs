using Catalog.Api.Constants;
using Catalog.Application.Features.Product.Queries;
using Catalog.Application.Models.Results;
using Common.Models.Reponses;

namespace Catalog.Api.Endpoints;

public sealed class GetProductById : ICarterModule
{
    #region Implementations

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet(ApiRoutes.Product.GetProductById, HandleGetProductAsync)
            .WithTags(ApiRoutes.Product.Tags)
            .WithName(nameof(GetProductById))
            .Produces<ApiGetResponse<GetProductByIdResult>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization();
    }

    #endregion

    #region Methods

    private async Task<ApiGetResponse<GetProductByIdResult>> HandleGetProductAsync(
        ISender sender,
        Guid productId,
        CancellationToken cancellationToken)
    {
        var query = new GetProductByIdQuery(productId);
        var result = await sender.Send(query, cancellationToken);
        return new ApiGetResponse<GetProductByIdResult>(result);
    }

    #endregion
}
