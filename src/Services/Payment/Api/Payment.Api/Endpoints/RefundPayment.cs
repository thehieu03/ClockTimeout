using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Payment.Application.Features.Payment.Commands;
using Common.ValueObjects;

namespace Payment.Api.Endpoints;

public class RefundPayment : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/payments/{paymentId}/refund", HandleAsync)
            .WithTags("Payments")
            .RequireAuthorization("Admin"); // Ensure 'Admin' policy exists or is configured
    }

    private async Task<IResult> HandleAsync(
        Guid paymentId,
        [FromBody] RefundRequest req,
        ISender sender)
    {
        var command = new RefundPaymentCommand(
            PaymentId: paymentId,
            Reason: req.Reason,
            Actor: Actor.System("System") // Or retrieve from user claims if HttpContext is available
        );

        var result = await sender.Send(command);

        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }
}

public record RefundRequest(string Reason);
