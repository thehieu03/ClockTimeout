using Catalog.Infrastructure.Data;
using Marten;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Minio;

namespace Catalog.Infrastructure;
#region Methods
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration cfg)
    {
        services.AddMarten(options =>
        {
            options.Connection(cfg[$"{ConnectionStringsCfg.Section}:{ConnectionStringsCfg.Database}"]!);
            options.UseSystemTextJsonForSerialization();
        }).UseLightweightSessions();
        // EF core (new relational database)
        var connectionString = cfg[$"{ConnectionStringsCfg.Section}:{ConnectionStringsCfg.Database}"];
        services.AddDbContext<CatalogDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });
        services.Scan(s => s
        .FromAssembliesOf(typeof(InfrastructureMarker))
        .AddClasses(c => c.Where(t => t.Name.EndsWith("Services")))
        .UsingRegistrationStrategy(Scrutor.RegistrationStrategy.Skip)
        .AsImplementedInterfaces()
        .WithScopedLifetime());
        services.Scan(s => s.FromAssembliesOf(typeof(InfrastructureMarker)
        ).AddClasses(c => c.Where(t => t.Name.EndsWith("Repository")))
        .UsingRegistrationStrategy(Scrutor.RegistrationStrategy.Skip)
        .AsImplementedInterfaces()
        .WithScopedLifetime());
        services.AddMinio(configureClient => configureClient.WithEndpoint(
            cfg[$"{MinIoCfg.Section}:{MinIoCfg.Endpoint}"]!
        ).WithCredentials(
            cfg[$"{MinIoCfg.Section}:{MinIoCfg.AccessKey}"]!,
            cfg[$"{MinIoCfg.Section}:{MinIoCfg.SecretKey}"]!
        ).WithSSL(cfg.GetValue<bool>($"{MinIoCfg.Section}:{MinIoCfg.Secure}"))
        .Build()
        );
        return services;
    }
    public static WebApplication UseInfrastructure(this WebApplication app)
    {
        return app;
    }
}
#endregion