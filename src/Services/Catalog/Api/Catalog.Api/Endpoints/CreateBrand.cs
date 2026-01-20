using AutoMapper;
using BuildingBlocks.Authentication.Extensions;
using BuildingBlocks.Extensions;
using Catalog.Api.Constants;
using Catalog.Api.Models;
using Catalog.Application.Dtos.Brands;
using Catalog.Application.Features.Brand.Commands;
using Common.Constants;
using Common.Models.Reponses;
using Common.ValueObjects;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.Api.Endpoints;

public sealed class CreateBrand : ICarterModule
{
    #region Methods

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost(ApiRoutes.Brand.Create, HandleCreateBrandAsync)
            .WithTags(ApiRoutes.Brand.Tags)
            .WithName(nameof(CreateBrand))
            .Produces<ApiCreatedResponse<Guid>>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .RequireAuthorization();
    }

    #endregion

    #region Methods

    private async Task<ApiCreatedResponse<Guid>> HandleCreateBrandAsync(
        ISender sender,
        IMapper mapper,
        IHttpContextAccessor httpContext,
        [FromBody] CreateBrandRequest req)
    {
        if (req == null) throw new ClientValidationException(MessageCode.BadRequest);

        var dto = mapper.Map<CreateBrandDto>(req);
        var currentUser = httpContext.GetCurrentUser();
        var command = new CreateBrandCommand(dto, Actor.User(currentUser.Email));
        var result = await sender.Send(command);
        return new ApiCreatedResponse<Guid>(result);
    }

    #endregion
}
