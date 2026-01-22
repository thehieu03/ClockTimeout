using Order.Domain.Repositories;

namespace Order.Domain.Abstractions;

public interface IUnitOfWork
{

    #region Fields, Properties and Indexers

    IOrderRepository Orders { get; }
    IOrderItemRepository OrderItems { get; }

    #endregion

    #region Methods
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<IDbTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
    #endregion
}