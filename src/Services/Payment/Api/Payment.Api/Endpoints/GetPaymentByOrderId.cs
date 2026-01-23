using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Payment.Api.Constants;
using Payment.Application.Dtos;
using Payment.Application.Features.Payment.Queries;

namespace Payment.Api.Endpoints;

public sealed class GetPaymentByOrderId : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet(ApiRoutes.Payment.GetByOrderId, HandleGetPaymentByOrderIdAsync)
            .WithTags(ApiRoutes.Payment.Tags)
            .WithName(nameof(GetPaymentByOrderId))
            .Produces<PaymentDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private async Task<IResult> HandleGetPaymentByOrderIdAsync(
        [FromServices] ISender sender,
        [FromRoute] Guid orderId)
    {
        var query = new GetPaymentByOrderIdQuery(orderId);
        var result = await sender.Send(query);
        return Results.Ok(result);
    }
}
