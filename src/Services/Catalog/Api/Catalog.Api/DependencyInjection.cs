using BuildingBlocks.DistributedTracing;

namespace Catalog.Api;

public static class DependencyInjection
{

    #region  Methods

    public static IServiceCollection AddApiServices(this IServiceCollection services,IConfiguration cfg)
    {
        services.AddDistributedTracing(cfg);
        // services.AddSerilogLogging(cfg);
        return services;
    }
    

    #endregion
}