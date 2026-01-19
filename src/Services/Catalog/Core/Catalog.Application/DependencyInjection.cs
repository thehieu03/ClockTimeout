using BuildingBlocks.Behaviors;
using BuildingBlocks.Extensions.Handler;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;
using System.Reflection;

namespace Catalog.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddExceptionHandler<CustomExceptionHandler>();
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            config.AddOpenBehavior(typeof(ValidationBehavior<,>));
            config.AddOpenBehavior(typeof(LoggingBehavior<,>));
        });
        services.AddFeatureManagement();
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        return services;
    }
}
