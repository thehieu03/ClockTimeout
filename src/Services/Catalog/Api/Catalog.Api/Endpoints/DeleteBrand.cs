using BuildingBlocks.Extensions;
using Catalog.Api.Constants;
using Catalog.Application.Features.Brand.Commands;
using Common.Constants;
using Common.Models.Reponses;

namespace Catalog.Api.Endpoints;

public sealed class DeleteBrand : ICarterModule
{
    #region Methods

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete(ApiRoutes.Brand.Delete, HandleDeleteBrandAsync)
            .WithTags(ApiRoutes.Brand.Tags)
            .WithName(nameof(DeleteBrand))
            .Produces<ApiDeletedResponse<Guid>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization();
    }

    #endregion

    #region Methods

    private async Task<ApiDeletedResponse<Guid>> HandleDeleteBrandAsync(
        ISender sender,
        Guid brandId,
        CancellationToken cancellationToken)
    {
        var command = new DeleteBrandCommand(brandId);
        await sender.Send(command, cancellationToken);
        return new ApiDeletedResponse<Guid>(brandId);
    }

    #endregion
}
