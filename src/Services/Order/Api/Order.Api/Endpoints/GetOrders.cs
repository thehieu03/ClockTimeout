using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Order.Api.Constants;
using Order.Application.Dtos.Orders;
using Order.Application.Features.Order.Queries;

namespace Order.Api.Endpoints;

public sealed class GetOrders : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet(ApiRoutes.Order.GetOrders, HandleGetOrdersAsync)
            .WithTags(ApiRoutes.Order.Tags)
            .WithName(nameof(GetOrders))
            .Produces<List<OrderDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .RequireAuthorization();
    }

    private async Task<IResult> HandleGetOrdersAsync(
        [FromServices] ISender sender)
    {
        var query = new GetOrdersQuery();
        var result = await sender.Send(query);
        return Results.Ok(result);
    }
}
