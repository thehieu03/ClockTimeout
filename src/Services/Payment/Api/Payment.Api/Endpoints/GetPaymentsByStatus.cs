using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Payment.Api.Constants;
using Payment.Application.Dtos;
using Payment.Application.Features.Payment.Queries;
using Payment.Domain.Enums;

namespace Payment.Api.Endpoints;

public sealed class GetPaymentsByStatus : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet(ApiRoutes.Payment.GetByStatus, HandleGetPaymentsByStatusAsync)
            .WithTags(ApiRoutes.Payment.Tags)
            .WithName(nameof(GetPaymentsByStatus))
            .Produces<IReadOnlyList<PaymentDto>>(StatusCodes.Status200OK);
    }

    private async Task<IResult> HandleGetPaymentsByStatusAsync(
        [FromServices] ISender sender,
        [FromRoute] PaymentStatus status)
    {
        var query = new GetPaymentsByStatusQuery(status);
        var result = await sender.Send(query);
        return Results.Ok(result);
    }
}
