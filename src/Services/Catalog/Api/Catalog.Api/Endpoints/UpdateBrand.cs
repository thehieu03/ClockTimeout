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

public sealed class UpdateBrand : ICarterModule
{
    #region Methods

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut(ApiRoutes.Brand.Update, HandleUpdateBrandAsync)
            .WithTags(ApiRoutes.Brand.Tags)
            .WithName(nameof(UpdateBrand))
            .Produces<ApiUpdatedResponse<Guid>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization();
    }

    #endregion

    #region Methods

    private async Task<ApiUpdatedResponse<Guid>> HandleUpdateBrandAsync(
        ISender sender,
        IMapper mapper,
        IHttpContextAccessor httpContext,
        Guid brandId,
        [FromBody] UpdateBrandRequest req)
    {
        if (req == null) throw new ClientValidationException(MessageCode.BadRequest);

        var dto = mapper.Map<UpdateBrandDto>(req);
        var currentUser = httpContext.GetCurrentUser();
        var command = new UpdateBrandCommand(brandId, dto, Actor.User(currentUser.Email));
        var result = await sender.Send(command);
        return new ApiUpdatedResponse<Guid>(result);
    }

    #endregion
}
