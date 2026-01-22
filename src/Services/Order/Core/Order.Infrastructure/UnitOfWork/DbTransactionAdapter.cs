using System.Data;
using Microsoft.EntityFrameworkCore.Storage;
using Order.Domain.Abstractions;


namespace Order.Infrastructure.UnitOfWork;

public class DbTransactionAdapter(IDbContextTransaction transaction):Order.Domain.Abstractions.IDbTransaction
{

    #region Impolementations
    public void Dispose()
    {
        transaction.Dispose();
    }
    public async ValueTask DisposeAsync()
    {
        await transaction.DisposeAsync();
    }
    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        await transaction.CommitAsync(cancellationToken);
    }
    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        await transaction.RollbackAsync(cancellationToken);
    }
    #endregion
}