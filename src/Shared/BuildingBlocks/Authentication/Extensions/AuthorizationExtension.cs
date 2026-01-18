using System.Security.Claims;
using System.Text.Json;
using Common.Configurations;
using Common.Constants;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace BuildingBlocks.Authentication.Extensions;

public static class AuthorizationExtension
{

    #region Methods
    public static IServiceCollection AddAuthenticationAndAuthorization(this IServiceCollection services, IConfiguration cfg)
    {
        var authority = cfg[$"{AuthorizationCfg.Section}:{AuthorizationCfg.Authority}"];
        var clientId = cfg[$"{AuthorizationCfg.Section}:{AuthorizationCfg.ClientId}"];
        var audience = cfg[$"{AuthorizationCfg.Section}:{AuthorizationCfg.Audience}"];
        var requireHttps = cfg.GetValue<bool>($"{AuthorizationCfg.Section}:{AuthorizationCfg.RequireHttpsMetadata}", true);

        // Add Authentication
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.Authority = authority;
            options.Audience = audience;
            options.RequireHttpsMetadata = requireHttps;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters()
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ClockSkew = TimeSpan.FromSeconds(60),
                NameClaimType = JwtRegisteredClaimNames.Sub,
                RoleClaimType = ClaimTypes.Role
            };
            options.Events = new JwtBearerEvents()
            {
                OnTokenValidated = ctx =>
                {
                    var id = ctx.Principal?.Identities as ClaimsIdentity;
                    if (id == null)
                    {
                        return Task.CompletedTask;
                    }
                    var realmAccess = ctx.Principal!.FindFirst(CustomClaimTypes.RealmAccess)?.Value;
                    if (realmAccess != null)
                    {
                        using var doc = JsonDocument.Parse(realmAccess);
                        if (doc.RootElement.TryGetProperty(CustomClaimTypes.Roles, out var roles))
                        {
                            foreach (var role in roles.EnumerateArray())
                            {
                                var roleName = role.GetString();
                                if (!string.IsNullOrEmpty(roleName))
                                {
                                    id.AddClaim(new Claim(ClaimTypes.Role, roleName));
                                }
                            }
                        }
                    }
                    return Task.CompletedTask;
                }
            };
        });
        services.AddAuthorization();
        return services;
    }
    #endregion
}