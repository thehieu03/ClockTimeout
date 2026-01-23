using Carter;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Order.Api.Constants;
using Order.Application.Dtos.Orders;
using Order.Application.Features.Order.Queries;

namespace Order.Api.Endpoints;

public sealed class GetOrderById : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet(ApiRoutes.Order.GetById, HandleGetOrderByIdAsync)
            .WithTags(ApiRoutes.Order.Tags)
            .WithName(nameof(GetOrderById))
            .Produces<OrderDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization();// Admin or Owner policy
    }

    private async Task<IResult> HandleGetOrderByIdAsync(
        [FromServices] ISender sender,
        [FromRoute] Guid orderId)
    {
        var query = new GetOrderByIdQuery(orderId);

        var result = await sender.Send(query);

        return Results.Ok(result);
    }
}
