namespace Catalog.Application.Services;

public interface ISeedDataServices
{
    Task<bool> SeedDataAsync(IDocumentSession session,CancellationToken cancellationToken);
}
