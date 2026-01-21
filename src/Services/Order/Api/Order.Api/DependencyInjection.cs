using Carter;
using Order.Application;
using Order.Infrastructure;

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
        // Register Carter
        services.AddCarter();
        return services;
    }
    public static WebApplication UseApi(this WebApplication app)
    {
        app.MapCarter();
        return app;
    }
    #endregion
    
}