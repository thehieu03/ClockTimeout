using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Payment.Api.Constants;
using Payment.Application.Dtos;
using Payment.Application.Features.Payment.Queries;

namespace Payment.Api.Endpoints;

public sealed class GetPaymentById : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet(ApiRoutes.Payment.GetById, HandleGetPaymentByIdAsync)
            .WithTags(ApiRoutes.Payment.Tags)
            .WithName(nameof(GetPaymentById))
            .Produces<PaymentDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private async Task<IResult> HandleGetPaymentByIdAsync(
        [FromServices] ISender sender,
        [FromRoute] Guid paymentId)
    {
        var query = new GetPaymentByIdQuery(paymentId);
        var result = await sender.Send(query);
        return Results.Ok(result);
    }
}
