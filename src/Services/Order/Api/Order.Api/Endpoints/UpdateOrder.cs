using BuildingBlocks.Authentication.Extensions;
using Carter;
using Common.Models.Reponses;
using Common.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Order.Api.Constants;
using Order.Application.Dtos.Orders;
using Order.Application.Features.Order.Commands;

namespace Order.Api.Endpoints;

public sealed class UpdateOrder : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut(ApiRoutes.Order.Update, HandleUpdateOrderAsync)
            .WithTags(ApiRoutes.Order.Tags)
            .WithName(nameof(UpdateOrder))
            .Produces<ApiUpdatedResponse<Guid>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private async Task<ApiUpdatedResponse<Guid>> HandleUpdateOrderAsync(
        [FromServices] ISender sender,
        [FromServices] IHttpContextAccessor httpContextAccessor,
        [FromRoute] Guid orderId,
        [FromBody] CreateOrUpdateOrderDto dto)
    {
        var currentUser = httpContextAccessor.GetCurrentUser();
        var actor = Actor.User(currentUser.Email);

        var command = new UpdateOrderCommand(orderId, dto, actor);
        var result = await sender.Send(command);

        return new ApiUpdatedResponse<Guid>(result);
    }
}
