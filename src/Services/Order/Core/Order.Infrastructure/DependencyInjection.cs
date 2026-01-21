using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Order.Infrastructure;

public static class DependencyInjection
{

    #region Methods

    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services,IConfiguration configuration)
    {
        var connectionString=configuration.GetConnectionString("OrderDb");
        return services;
    }
    

    #endregion
}