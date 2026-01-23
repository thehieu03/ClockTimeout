using Payment.Domain.Repositories;

namespace Payment.Domain.Abstractions;

public interface IUnitOfWork
{
    IPaymentRepository Payments { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<IDbTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}
