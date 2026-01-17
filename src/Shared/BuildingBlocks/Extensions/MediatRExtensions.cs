using BuildingBlocks.Behaviors;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace BuildingBlocks.Extensions;

public static class MediatRExtensions
{
    #region Method
    public static IServiceCollection AddMediatRWithBehaviors(this IServiceCollection services,
        Assembly assembly)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddBehavior(typeof(ValidationBehavior<,>));
            cfg.AddBehavior(typeof(LoggingBehavior<,>));
        });
        return services;
    }
    #endregion
    public static IServiceCollection AddMediaRWithbehaviors(this IServiceCollection services,
        params Assembly[] assemblies)
    {
        services.AddMediatR(cfg =>
        {
            foreach (var assembly in assemblies)
            {
                cfg.RegisterServicesFromAssembly(assembly);
            }
            cfg.AddBehavior(typeof(ValidationBehavior<,>));
            cfg.AddBehavior(typeof(LoggingBehavior<,>));
        });
        return services;
    }
}
