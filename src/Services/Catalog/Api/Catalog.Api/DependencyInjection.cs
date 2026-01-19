using BuildingBlocks.Authentication.Extensions;
using BuildingBlocks.DistributedTracing;
using BuildingBlocks.Logging;
using BuildingBlocks.Swagger.Extensions;
using Carter;
using Common.Configurations;
using Common.Constants;
using Common.Models.Reponses;
using HealthChecks.MySql;
using HealthChecks.NpgSql;
using HealthChecks.SqlServer;
using HealthChecks.UI.Client;
using JasperFx.MultiTenancy;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Catalog.Api;

public static class DependencyInjection
{

    #region  Methods

    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration cfg)
    {
        services.AddDistributedTracing(cfg);
        services.AddSerilogLogging(cfg);
        services.AddCarter();
        // HealthChecks
        {
            var dbype = cfg[$"{ConnectionStringsCfg.Section}:{ConnectionStringsCfg.DbType}"];
            var conn = cfg[$"{ConnectionStringsCfg.Section}:{ConnectionStringsCfg.Database}"];
            switch (dbype)
            {
                case DatabaseType.SqlServer:
                    services.AddHealthChecks()
                        .AddSqlServer(connectionString: conn!);
                    break;
                case DatabaseType.MySql:
                    services.AddHealthChecks()
                        .AddMySql(connectionString: conn!);
                    break;
                case DatabaseType.Postgres:
                    services.AddHealthChecks()
                        .AddNpgSql(connectionString: conn!);
                    break;
                default:
                    throw new Exception("Unsupported database type");
            }
        }
        services.AddHttpContextAccessor();
        services.AddAuthenticationAndAuthorization(cfg);
        services.AddSwaggerServices(cfg);
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        return services;
    }
    public static WebApplication UseApi(this WebApplication app)
    {
        app.UseSerilogReqLogging();
        app.UsePrometheusEnpoint();
        app.MapCarter();
        app.UseExceptionHandler(options => { });
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseSwaggerApi();
        app.MapGet("/", (IWebHostEnvironment env) => new ApiDefaultPathResponse
        {
            Services="Catalog.Api",
            Status="Running",
            TimeStamp=DateTimeOffset.UtcNow,
            Environment=env.EnvironmentName,
            Endpoints=new Dictionary<string, string>
            {
                {"health","/health" },
            }
            ,Message="Api is running..."
        });
        return app;
    }

    #endregion
}