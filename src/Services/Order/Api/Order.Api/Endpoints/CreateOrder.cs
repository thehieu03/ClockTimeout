using BuildingBlocks.Authentication.Extensions;
using Carter;
using Microsoft.AspNetCore.Mvc;
using Common.Models.Reponses;
using Common.ValueObjects;
using MediatR;
using Order.Api.Constants;
using Order.Application.Dtos.Orders;
using Order.Application.Features.Order.Commands;

namespace Order.Api.Endpoints;

public sealed class CreateOrder:ICarterModule
{

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost(ApiRoutes.Order.Create,HandleCreateOrderAsync)
            .WithTags(ApiRoutes.Order.Tags)
            .WithName(nameof(CreateOrder))
            .Produces<ApiCreatedResponse<Guid>>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status400BadRequest);
    }
    private async Task<ApiCreatedResponse<Guid>> HandleCreateOrderAsync(ISender sender,HttpContextAccessor httpContext,[FromBody]CreateOrUpdateOrderDto dto)
    {
        // Get current user
        var currentUser = httpContext.GetCurrentUser();
        var actor = Actor.User(currentUser.Email);
        // create command
        var command = new CreateOrderCommand(dto, actor);
        // Send command via MediatR
        var result = await sender.Send(command);
        return new ApiCreatedResponse<Guid>(result);
    }

}