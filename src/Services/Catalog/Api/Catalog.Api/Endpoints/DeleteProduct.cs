using BuildingBlocks.Extensions;
using Catalog.Api.Constants;
using Catalog.Application.Features.Product.Commands;
using Common.Constants;
using Common.Models.Reponses;

namespace Catalog.Api.Endpoints;

public sealed class DeleteProduct : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete(ApiRoutes.Product.Delete, HandleDeleteProductAsync)
            .WithTags(ApiRoutes.Product.Tags)
            .WithName(nameof(DeleteProduct))
            .Produces<ApiDeletedResponse<Guid>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization();
    }

    private async Task<ApiDeletedResponse<Guid>> HandleDeleteProductAsync(
        ISender sender,
        Guid productId,
        CancellationToken cancellationToken)
    {
        var command = new DeleteProductCommand(productId);
        await sender.Send(command, cancellationToken);
        return new ApiDeletedResponse<Guid>(productId);
    }
}
