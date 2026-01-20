namespace Catalog.Application.Services;

public interface ISeedDataServices
{
    #region Methods
    Task<bool> SeedDataAsync(IDocumentSession session,CancellationToken cancellationToken);
    #endregion
}
