using Microsoft.EntityFrameworkCore.Storage;
using Payment.Domain.Abstractions;
using Payment.Domain.Repositories;
using Payment.Infrastructure.Data;

namespace Payment.Infrastructure;

public class UnitOfWork(ApplicationDbContext context, IPaymentRepository paymentRepository) : IUnitOfWork
{
    private IDbContextTransaction? _currentTransaction;

    public IPaymentRepository Payments => paymentRepository;

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IDbTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _currentTransaction = await context.Database.BeginTransactionAsync(cancellationToken);
        return new DbTransactionWrapper(_currentTransaction);
    }
}

internal class DbTransactionWrapper(IDbContextTransaction transaction) : IDbTransaction
{
    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        await transaction.RollbackAsync(cancellationToken);
    }

    public void Dispose()
    {
        transaction.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await transaction.DisposeAsync();
    }
}
