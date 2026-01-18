using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BuildingBlocks.Swagger;

public class AuthorizeCheckOperationFilter : IOperationFilter
{

    #region Methods

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var hasAuthorize = context.ApiDescription
            .ActionDescriptor
            .EndpointMetadata
            .OfType<IAllowAnonymous>()
            .Any();
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
                        Id = "oauth2"
                    }
                }] = new[]
                {
                    "openid"
                }
            }
        };
    }

    #endregion

}