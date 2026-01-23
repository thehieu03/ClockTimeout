using Carter;
using Order.Application;
using Order.Infrastructure;
using BuildingBlocks.Authentication.Extensions;
using BuildingBlocks.Extensions.Handler;

namespace Order.Api;

public static class DependencyInjection
{

    #region Methods

    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration cfg)
    {
        // Register Application Services
        services.AddApplicationServices();
        // Register Infrastructure Services
        services.AddInfrastructureServices(cfg);
        // Register Authentication and Authorization
        services.AddAuthenticationAndAuthorization(cfg);
        // Register HttpContextAccessor
        services.AddHttpContextAccessor();
        // Register Exception Handler
        services.AddExceptionHandler<CustomExceptionHandler>();
        services.AddProblemDetails();
        // Register Carter
        services.AddCarter();
        return services;
    }
    public static WebApplication UseApi(this WebApplication app)
    {
        app.MapCarter();
        app.UseExceptionHandler(options => { });
        app.UseAuthentication();
        app.UseAuthorization();
        return app;
    }
    #endregion
    
}