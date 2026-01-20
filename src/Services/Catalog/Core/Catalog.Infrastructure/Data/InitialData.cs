using Catalog.Application.Services;
using Marten;
using Marten.Schema;
using Microsoft.Extensions.DependencyInjection;
using Polly;

namespace Catalog.Infrastructure.Data;

public sealed class InitialData : IInitialData
{
    #region Fields, Properties and Indexers
    private readonly IServiceProvider _serviceProvider;
    #endregion

    #region ctor
    public InitialData(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    #endregion
    #region Implementions
    public async Task Populate(IDocumentStore store, CancellationToken cancellation)
    {
        var retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(5, i => TimeSpan.FromSeconds(5 * i));
        var circuitBreakerPolicy = Policy
            .Handle<Exception>()
            .CircuitBreakerAsync(2, TimeSpan.FromSeconds(30));
        await retryPolicy
            .WrapAsync(circuitBreakerPolicy)
            .ExecuteAsync(async (ct) =>
            {
                await using var session = store.LightweightSession();
                using var scope = _serviceProvider.CreateScope();
                var seedDataService = scope.ServiceProvider.GetRequiredService<ISeedDataServices>();
                await seedDataService.SeedDataAsync(session, ct);
            }, cancellation);
    }
    #endregion
}
