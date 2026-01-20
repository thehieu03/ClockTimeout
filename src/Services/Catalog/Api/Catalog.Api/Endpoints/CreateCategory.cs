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

public sealed class CreateCategory : ICarterModule
{
    #region Methods

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost(ApiRoutes.Category.Create, HandleCreateCategoryAsync)
            .WithTags(ApiRoutes.Category.Tags)
            .WithName(nameof(CreateCategory))
            .Produces<ApiCreatedResponse<Guid>>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .RequireAuthorization();
    }

    #endregion

    #region Methods

    private async Task<ApiCreatedResponse<Guid>> HandleCreateCategoryAsync(
        ISender sender,
        IMapper mapper,
        IHttpContextAccessor httpContext,
        [FromBody] CreateCategoryRequest req)
    {
        if (req == null) throw new ClientValidationException(MessageCode.BadRequest);

        var dto = mapper.Map<CreateCategoryDto>(req);
        var currentUser = httpContext.GetCurrentUser();
        var command = new CreateCategoryCommand(dto, Actor.User(currentUser.Email));
        var result = await sender.Send(command);
        return new ApiCreatedResponse<Guid>(result);
    }

    #endregion
}
