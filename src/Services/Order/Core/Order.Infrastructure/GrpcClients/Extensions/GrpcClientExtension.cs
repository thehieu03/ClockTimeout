using System.Runtime.InteropServices;
using Common.Configurations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Order.Infrastructure.GrpcClients.Extensions;

public static class GrpcClientExtension
{

    #region Methods

    public static IServiceCollection AddGrpcClients(this IServiceCollection services, IConfiguration cfg)
    {
        var catalogServicesUrl = cfg.GetValue<string>($"{GrpcClientCfg.Catalog.Section}:{GrpcClientCfg.Catalog.Url}")
            ?? throw new InvalidOleVariantTypeException("Catalog service url is not configured");
        // services.AddGrpcClients<Cat>()   
        return services;
    }

    #endregion
}