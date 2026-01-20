using AutoMapper;
using BuildingBlocks.Authentication.Extensions;
using BuildingBlocks.Extensions;
using BuildingBlocks.Swagger.Extensions;
using Catalog.Api.Constants;
using Catalog.Api.Models;
using Catalog.Application.Dtos.Products;
using Catalog.Application.Features.Product.Commands;
using Common.Constants;
using Common.Models;
using Common.Models.Reponses;
using Common.ValueObjects;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.Api.Endpoints;

public sealed class UpdateProduct : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut(ApiRoutes.Product.Update, HandleUpdateProductAsync)
            .WithTags(ApiRoutes.Product.Tags)
            .WithName(nameof(UpdateProduct))
            .WithMultipartForm<UpdateProductRequest>()
            .Produces<ApiUpdatedResponse<Guid>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .RequireAuthorization();
    }

    private async Task<ApiUpdatedResponse<Guid>> HandleUpdateProductAsync(
        ISender sender,
        IMapper mapper,
        IHttpContextAccessor httpContext,
        Guid productId,
        [FromForm] UpdateProductRequest req)
    {
        if (req == null) throw new ClientValidationException(MessageCode.BadRequest);

        // Handle form files if not bound automatically
        if ((req.ImageFiles == null || req.ImageFiles.Count == 0) && httpContext.HttpContext != null)
        {
            req.ImageFiles = httpContext.HttpContext.Request.Form.Files.ToList();
        }

        // Map request to DTO
        var dto = mapper.Map<UpdateProductDto>(req);
        dto.CategoryIds = req.CategoryIds?.Select(Guid.Parse).ToList();

        // Convert image files to UploadFileBytes
        if (req.ImageFiles != null && req.ImageFiles.Count > 0)
        {
            dto.UploadImages ??= new();
            foreach (var file in req.ImageFiles!)
            {
                using var ms = new MemoryStream();
                await file.CopyToAsync(ms);
                dto.UploadImages.Add(new UploadFileBytes
                {
                    FileName = file.FileName,
                    Bytes = ms.ToArray(),
                    ContentType = file.ContentType
                });
            }
        }

        // Convert thumbnail file to UploadFileBytes
        if (req.ThumbnailFile != null && req.ThumbnailFile.Length > 0)
        {
            using var ms = new MemoryStream();
            await req.ThumbnailFile.CopyToAsync(ms);

            dto.UploadThumbnail = new UploadFileBytes
            {
                FileName = req.ThumbnailFile.FileName,
                Bytes = ms.ToArray(),
                ContentType = req.ThumbnailFile.ContentType
            };
        }

        // Get current user và create Actor
        var currentUser = httpContext.GetCurrentUser();
        var command = new UpdateProductCommand(productId, dto, Actor.User(currentUser.Email));

        // Send command
        var result = await sender.Send(command);

        return new ApiUpdatedResponse<Guid>(result);
    }
}
