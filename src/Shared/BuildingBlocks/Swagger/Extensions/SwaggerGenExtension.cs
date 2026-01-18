using System.Security.Policy;
using Common.Configurations;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BuildingBlocks.Swagger.Extensions;

public static class SwaggerGenExtension
{

    #region Methods

    public static IServiceCollection AddSwaggerServices(this IServiceCollection services, IConfiguration cfg)
    {
        var authority = cfg[$"{AuthorizationCfg.Section}:{AuthorizationCfg.Authority}"];
        var clientId = cfg[$"{AuthorizationCfg.Section}:{AuthorizationCfg.ClientId}"];
        var clientSecret = cfg[$"{AuthorizationCfg.Section}:{AuthorizationCfg.ClientSecret}"];
        var scopesArray = cfg.GetValue<string[]>($"{AuthorizationCfg.Section}:{AuthorizationCfg.Scope}");
        var oauthScopes = scopesArray?.ToDictionary(s => s, s => $"OpenID scope{s}");
        var authUrl = new Url($"{authority}/protocol/openid-connect/auth");
        var tokenUrl = new Url($"{authority}/protocol/openid-connect/token");
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(otps =>
        {
            otps.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = cfg[$"{AppConfigCfg.Section}:{AppConfigCfg.ServiceName}"],
                Version = "v1",
                Description = $"This is an API for {cfg[$"{AppConfigCfg.Section}:{AppConfigCfg.ServiceName}"]}",
                Contact = new OpenApiContact
                {
                    Name = cfg[$"{SwaggerGenCfg.Section}:{SwaggerGenCfg.ContactName}"],
                    Email = cfg[$"{SwaggerGenCfg.Section}:{SwaggerGenCfg.ContactEmail}"],
                    Url = new Uri(cfg[$"{SwaggerGenCfg.Section}:{SwaggerGenCfg.ContactUrl}"])
                }
            });
            otps.AddSecurityDefinition("Beader",new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                Description = "Enter 'Beader {token}'"
            });
            otps.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                [new OpenApiSecurityScheme()
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Beader"
                    }
                }]=Array.Empty<string>()
            });
            otps.OperationFilter<AuthorizeCheckOperationFilter>();
        });
        return services;
    }
    public static WebApplication UseSwaggerApi(this WebApplication app)
    {
        var cfg = app.Configuration;
        if (!cfg.GetValue<bool>($"{SwaggerGenCfg.Section}:{SwaggerGenCfg.Enable}")) return app;
        var clientId = cfg[$"{AuthorizationCfg.Section}:{AuthorizationCfg.ClientId}"];
        var clientSecret=cfg[$"{AuthorizationCfg.Section}:{AuthorizationCfg.ClientSecret}"];
        var scopes = cfg.GetValue<string[]>($"{AuthorizationCfg.Section}:{AuthorizationCfg.Scope}");
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json","v1");
            c.OAuthClientId(clientId);
            c.OAuthClientSecret(clientSecret);
            c.OAuthScopes(scopes);
            c.OAuth2RedirectUrl(cfg[$"{AuthorizationCfg.Section}:{AuthorizationCfg.OAuth2RedirectUrl}"]);
        });
        return app;
    }

    #endregion
}