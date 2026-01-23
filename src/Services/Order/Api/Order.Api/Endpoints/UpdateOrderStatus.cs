using BuildingBlocks.Authentication.Extensions;
using Carter;
using Common.Models.Reponses;
using Common.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Order.Api.Constants;
using Order.Api.Models;
using Order.Application.Features.Order.Commands;

namespace Order.Api.Endpoints;

public sealed class UpdateOrderStatus : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch(ApiRoutes.Order.UpdateOrderStatus, HandleUpdateOrderStatusAsync)
            .WithTags(ApiRoutes.Order.Tags)
            .WithName(nameof(UpdateOrderStatus))
            .Produces<ApiUpdatedResponse<Guid>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization();
    }

    private async Task<ApiUpdatedResponse<Guid>> HandleUpdateOrderStatusAsync(
        ISender sender,
        IHttpContextAccessor httpContext,
        [FromRoute] Guid orderId,
        [FromBody] UpdateOrderStatusRequest request)
    {
        var currentUser = httpContext.GetCurrentUser();
        var command = new UpdateOrderStatusCommand(
            orderId,
            request.Status,
            request.Reason,
            Actor.User(currentUser.Email));

        var result = await sender.Send(command);

        return new ApiUpdatedResponse<Guid>(result);
    }
}
