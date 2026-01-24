using Carter;
using Common.Models.Reponses;
using Common.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Payment.Api.Constants;
using Payment.Application.Features.Payment.Commands;
using Payment.Application.Models.Results;

namespace Payment.Api.Endpoints;

public sealed class ProcessPayment : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost(ApiRoutes.Payment.Process, HandleProcessPaymentAsync)
            .WithTags(ApiRoutes.Payment.Tags)
            .WithName(nameof(ProcessPayment))
            .Produces<ApiPerformedResponse<ProcessPaymentResult>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithDescription(EndpointDescriptions.Payment.Process)
            .RequireAuthorization();
    }

    private async Task<IResult> HandleProcessPaymentAsync(
        ISender sender,
        Guid paymentId,
        [FromBody] ProcessPaymentRequest? request,
        HttpContext httpContext)
    {
        var userId = httpContext.User.FindFirst("sub")?.Value
            ?? httpContext.User.FindFirst("userId")?.Value
            ?? "anonymous";

        var actor = Actor.User(userId);

        var command = new ProcessPaymentCommand(
            PaymentId: paymentId,
            ReturnUrl: request?.ReturnUrl,
            CancelUrl: request?.CancelUrl,
            Actor: actor
        );

        var result = await sender.Send(command);

        return Results.Ok(new ApiPerformedResponse<ProcessPaymentResult>(result));
    }
}

public record ProcessPaymentRequest(
    string? ReturnUrl = null,
    string? CancelUrl = null
);
