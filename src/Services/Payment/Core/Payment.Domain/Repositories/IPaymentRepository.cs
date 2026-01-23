using Payment.Domain.Entities;
using Payment.Domain.Enums;

namespace Payment.Domain.Repositories;

public interface IPaymentRepository
{
    Task<PaymentEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PaymentEntity?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PaymentEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PaymentEntity>> GetByStatusAsync(PaymentStatus status, CancellationToken cancellationToken = default);
    Task AddAsync(PaymentEntity entity, CancellationToken cancellationToken = default);
    void Update(PaymentEntity entity);
}
