using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Payment.Api.Constants;
using Payment.Application.Dtos;
using Payment.Application.Features.Payment.Queries;

namespace Payment.Api.Endpoints;

public sealed class GetPayments : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet(ApiRoutes.Payment.GetAll, HandleGetPaymentsAsync)
            .WithTags(ApiRoutes.Payment.Tags)
            .WithName(nameof(GetPayments))
            .Produces<IReadOnlyList<PaymentDto>>(StatusCodes.Status200OK);
    }

    private async Task<IResult> HandleGetPaymentsAsync(
        [FromServices] ISender sender)
    {
        var query = new GetPaymentsQuery();
        var result = await sender.Send(query);
        return Results.Ok(result);
    }
}
