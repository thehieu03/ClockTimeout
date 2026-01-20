using Catalog.Application.Services;
using Catalog.Infrastructure.Data;
using Catalog.Infrastructure.Services;
using Marten;
using Marten.Schema;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Minio;

namespace Catalog.Infrastructure;
#region Methods
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration cfg)
    {
        // Register InitialData as singleton (needs IServiceProvider)
        services.AddSingleton<IInitialData>(sp => new InitialData(sp));

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
        
        // Scan and register all Services (including SeedDataServices)
        services.Scan(s => s
        .FromAssembliesOf(typeof(InfrastructureMarker))
        .AddClasses(c => c.Where(t => t.Name.EndsWith("Services")))
        .UsingRegistrationStrategy(Scrutor.RegistrationStrategy.Append)
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
        // Initialize seed data automatically on startup
        // This will check if database has data and seed if empty
        var documentStore = app.Services.GetRequiredService<IDocumentStore>();
        var initialData = app.Services.GetRequiredService<IInitialData>();
        var logger = app.Services.GetRequiredService<ILogger<InitialData>>();

        // Run seed data initialization synchronously on startup
        // This ensures data is seeded before the API starts accepting requests
        try
        {
            logger.LogInformation("Starting seed data initialization...");
            initialData.Populate(documentStore, CancellationToken.None).GetAwaiter().GetResult();
            logger.LogInformation("Seed data initialization completed successfully.");
        }
        catch (Exception ex)
        {
            // Log error but don't fail startup - allows API to start even if seeding fails
            logger.LogError(ex, "Error seeding initial data. The API will continue to start.");
        }

        return app;
    }
}
#endregion