using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Authentication.Extensions;

public static class UserContextExtenstion
{
    public static  IServiceCollection AddAuthenticationAndAuthorization(this IServiceCollection services,IServiceCollection cfg)
    {
        // var authority=cfg[$"{}"]
        return services;
    }
}