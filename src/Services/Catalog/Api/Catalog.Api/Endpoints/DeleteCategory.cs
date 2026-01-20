using BuildingBlocks.Extensions;
using Catalog.Api.Constants;
using Catalog.Application.Features.Category.Commands;
using Common.Constants;
using Common.Models.Reponses;

namespace Catalog.Api.Endpoints;

public sealed class DeleteCategory : ICarterModule
{
    #region Methods

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete(ApiRoutes.Category.Delete, HandleDeleteCategoryAsync)
            .WithTags(ApiRoutes.Category.Tags)
            .WithName(nameof(DeleteCategory))
            .Produces<ApiDeletedResponse<Guid>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization();
    }

    #endregion

    #region Methods

    private async Task<ApiDeletedResponse<Guid>> HandleDeleteCategoryAsync(
        ISender sender,
        Guid categoryId,
        CancellationToken cancellationToken)
    {
        var command = new DeleteCategoryCommand(categoryId);
        await sender.Send(command, cancellationToken);
        return new ApiDeletedResponse<Guid>(categoryId);
    }

    #endregion
}
