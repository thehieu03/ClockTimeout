using AutoMapper;
using BuildingBlocks.Authentication.Extensions;
using BuildingBlocks.Extensions;
using Catalog.Api.Constants;
using Catalog.Api.Models;
using Catalog.Application.Dtos.Categories;
using Catalog.Application.Features.Category.Commands;
using Common.Constants;
using Common.Models.Reponses;
using Common.ValueObjects;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.Api.Endpoints;

public sealed class UpdateCategory : ICarterModule
{
    #region Methods

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut(ApiRoutes.Category.Update, HandleUpdateCategoryAsync)
            .WithTags(ApiRoutes.Category.Tags)
            .WithName(nameof(UpdateCategory))
            .Produces<ApiUpdatedResponse<Guid>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization();
    }

    #endregion

    #region Methods

    private async Task<ApiUpdatedResponse<Guid>> HandleUpdateCategoryAsync(
        ISender sender,
        IMapper mapper,
        IHttpContextAccessor httpContext,
        Guid categoryId,
        [FromBody] UpdateCategoryRequest req)
    {
        if (req == null) throw new ClientValidationException(MessageCode.BadRequest);

        var dto = mapper.Map<UpdateCategoryDto>(req);
        var currentUser = httpContext.GetCurrentUser();
        var command = new UpdateCategoryCommand(categoryId, dto, Actor.User(currentUser.Email));
        var result = await sender.Send(command);
        return new ApiUpdatedResponse<Guid>(result);
    }

    #endregion
}
