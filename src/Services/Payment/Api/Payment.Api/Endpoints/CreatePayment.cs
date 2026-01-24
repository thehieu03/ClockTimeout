using Carter;
using Common.Models.Reponses;
using Common.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Payment.Api.Constants;
using Payment.Application.Dtos;
using Payment.Application.Features.Payment.Commands;
using Payment.Domain.Enums;

namespace Payment.Api.Endpoints;

public sealed class CreatePayment : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost(ApiRoutes.Payment.Create, HandleCreatePaymentAsync)
            .WithTags(ApiRoutes.Payment.Tags)
            .WithName(nameof(CreatePayment))
            .Produces<ApiCreatedResponse<PaymentDto>>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Create a new payment for an order")
            .RequireAuthorization();
    }

    private async Task<IResult> HandleCreatePaymentAsync(
        ISender sender,
        [FromBody] CreatePaymentRequest request,
        HttpContext httpContext)
    {
        // Get current user from claims
        var userId = httpContext.User.FindFirst("sub")?.Value 
            ?? httpContext.User.FindFirst("userId")?.Value 
            ?? "anonymous";
        
        var actor = Actor.User(userId);

        var command = new CreatePaymentCommand(
            OrderId: request.OrderId,
            Amount: request.Amount,
            Method: request.Method,
            Actor: actor
        );

        var result = await sender.Send(command);

        return Results.Created(
            $"{ApiRoutes.Payment.Create}/{result.Id}",
            new ApiCreatedResponse<PaymentDto>(result)
        );
    }
}

public record CreatePaymentRequest(
    Guid OrderId,
    decimal Amount,
    PaymentMethod Method
);
