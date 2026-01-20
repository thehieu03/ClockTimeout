using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BuildingBlocks.Swagger;

public class AuthorizeCheckOperationFilter : IOperationFilter
{

    #region Methods

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        try
        {
            // Check if endpoint allows anonymous access
            var hasAllowAnonymous = context.ApiDescription
                .ActionDescriptor
                .EndpointMetadata
                .OfType<IAllowAnonymous>()
                .Any();

            // If anonymous is allowed, don't add security requirements
            if (hasAllowAnonymous) return;

            // Check if endpoint requires authorization
            var hasAuthorize = context.ApiDescription
                .ActionDescriptor
                .EndpointMetadata
                .OfType<IAuthorizeData>()
                .Any();

            // Only add security requirements if authorization is required
            if (!hasAuthorize) return;

            operation.Responses.Add("401", new OpenApiResponse
            {
                Description = "Unauthorized"
            });
            operation.Responses.Add("403", new OpenApiResponse
            {
                Description = "Forbidden"
            });
            operation.Security = new List<OpenApiSecurityRequirement>
            {
                new OpenApiSecurityRequirement
                {
                    [new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"  // Match security definition name
                        }
                    }] = Array.Empty<string>()
                }
            };
        }
        catch
        {
            // Silently ignore errors to prevent Swagger generation from failing
            // This can happen with certain endpoint types or metadata configurations
        }
    }

    #endregion

}