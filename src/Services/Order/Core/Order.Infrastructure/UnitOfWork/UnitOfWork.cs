using Order.Domain.Abstractions;
using Order.Domain.Repositories;
using Order.Infrastructure.Data;

namespace Order.Infrastructure.UnitOfWork;

public class UnitOfWork(IOrderRepository orders,IOrderItemRepository orderItems,ApplicationDbContext context):IUnitOfWork
{

    public IOrderRepository Orders { get; } = orders;
    public IOrderItemRepository OrderItems { get; }=orderItems;
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
       return await context.SaveChangesAsync(cancellationToken);  
    }
    public async Task<IDbTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        var tx = await context.Database.BeginTransactionAsync(cancellationToken);
        return new DbTransactionAdapter(tx);
    }
}