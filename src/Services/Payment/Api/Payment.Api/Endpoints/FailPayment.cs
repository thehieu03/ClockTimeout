using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Payment.Api.Constants;
using Payment.Application.Dtos;
using Payment.Application.Features.Payment.Commands;

namespace Payment.Api.Endpoints;

public sealed class FailPayment : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost(ApiRoutes.Payment.Fail, HandleFailPaymentAsync)
            .WithTags(ApiRoutes.Payment.Tags)
            .WithName(nameof(FailPayment))
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private async Task<IResult> HandleFailPaymentAsync(
        [FromServices] ISender sender,
        [FromRoute] Guid paymentId,
        [FromBody] FailPaymentDto dto)
    {
        var command = new FailPaymentCommand(paymentId, dto.ErrorCode, dto.ErrorMessage);
        await sender.Send(command);
        return Results.NoContent();
    }
}
