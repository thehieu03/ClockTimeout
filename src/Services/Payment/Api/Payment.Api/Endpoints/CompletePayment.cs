using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Payment.Api.Constants;
using Payment.Application.Dtos;
using Payment.Application.Features.Payment.Commands;

namespace Payment.Api.Endpoints;

public sealed class CompletePayment : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost(ApiRoutes.Payment.Complete, HandleCompletePaymentAsync)
            .WithTags(ApiRoutes.Payment.Tags)
            .WithName(nameof(CompletePayment))
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private async Task<IResult> HandleCompletePaymentAsync(
        [FromServices] ISender sender,
        [FromRoute] Guid paymentId,
        [FromBody] CompletePaymentDto dto)
    {
        var command = new CompletePaymentCommand(paymentId, dto.TransactionId);
        await sender.Send(command);
        return Results.NoContent();
    }
}
