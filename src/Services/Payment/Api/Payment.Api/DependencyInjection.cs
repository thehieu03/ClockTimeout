using BuildingBlocks.Extensions.Handler;
using Carter;
using Payment.Application;
using Payment.Infrastructure;

namespace Payment.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration cfg)
    {
        // Register Application Services
        services.AddApplicationServices();
        // Register Infrastructure Services
        services.AddInfrastructureServices(cfg);

        // Register HttpContextAccessor
        services.AddHttpContextAccessor();

        // Register Exception Handler
        services.AddExceptionHandler<CustomExceptionHandler>();

        // Register Problem Details
        services.AddProblemDetails();

        // Register Carter
        services.AddCarter();

        return services;
    }

    public static WebApplication UseApi(this WebApplication app)
    {
        app.UseExceptionHandler();
        app.MapCarter();
        return app;
    }
}
