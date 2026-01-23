using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Payment.Api.Constants;
using Payment.Application.Dtos;
using Payment.Application.Features.Payment.Commands;

namespace Payment.Api.Endpoints;

public sealed class RefundPayment : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost(ApiRoutes.Payment.Refund, HandleRefundPaymentAsync)
            .WithTags(ApiRoutes.Payment.Tags)
            .WithName(nameof(RefundPayment))
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private async Task<IResult> HandleRefundPaymentAsync(
        [FromServices] ISender sender,
        [FromRoute] Guid paymentId,
        [FromBody] RefundPaymentDto dto)
    {
        var command = new RefundPaymentCommand(paymentId, dto.RefundReason, dto.RefundTransactionId);
        await sender.Send(command);
        return Results.NoContent();
    }
}
