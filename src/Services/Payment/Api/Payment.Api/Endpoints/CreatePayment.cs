using Carter;
using Common.Models.Reponses;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Payment.Api.Constants;
using Payment.Application.Dtos;
using Payment.Application.Features.Payment.Commands;

namespace Payment.Api.Endpoints;

public sealed class CreatePayment : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost(ApiRoutes.Payment.Create, HandleCreatePaymentAsync)
            .WithTags(ApiRoutes.Payment.Tags)
            .WithName(nameof(CreatePayment))
            .Produces<ApiCreatedResponse<Guid>>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest);
    }

    private async Task<IResult> HandleCreatePaymentAsync(
        [FromServices] ISender sender,
        [FromBody] CreatePaymentDto dto)
    {
        var command = new CreatePaymentCommand(dto);
        var result = await sender.Send(command);
        return Results.Created($"/admin/payments/{result}", new ApiCreatedResponse<Guid>(result));
    }
}
