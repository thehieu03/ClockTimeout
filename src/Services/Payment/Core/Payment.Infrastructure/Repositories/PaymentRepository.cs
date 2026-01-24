using Microsoft.EntityFrameworkCore;
using Payment.Domain.Entities;
using Payment.Domain.Enums;
using Payment.Domain.Repositories;
using Payment.Infrastructure.Data;

namespace Payment.Infrastructure.Repositories;

public class PaymentRepository(ApplicationDbContext context) : IPaymentRepository
{
    private readonly DbSet<PaymentEntity> _dbSet = context.Set<PaymentEntity>();

    public async Task<PaymentEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync([id], cancellationToken);
    }

    public async Task<PaymentEntity?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(x => x.OrderId == orderId, cancellationToken);
    }

    public async Task<PaymentEntity?> GetByTransactionIdAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(x => x.TransactionId == transactionId, cancellationToken);
    }

    public async Task<IReadOnlyList<PaymentEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.OrderByDescending(x => x.CreatedOnUtc).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PaymentEntity>> GetByStatusAsync(PaymentStatus status, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(x => x.Status == status)
            .OrderByDescending(x => x.CreatedOnUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(PaymentEntity entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
    }

    public void Update(PaymentEntity entity)
    {
        _dbSet.Update(entity);
    }
}
