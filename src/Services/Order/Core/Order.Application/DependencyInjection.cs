using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Order.Application;

public static class DependencyInjection
{
    #region Methods
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();
        // Register MediaR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(assembly));
        // Register FluentValidation
        services.AddValidatorsFromAssembly(assembly);
        // Register AutoMapper
        services.AddAutoMapper(assembly);
        return services;
    }
    #endregion
}
