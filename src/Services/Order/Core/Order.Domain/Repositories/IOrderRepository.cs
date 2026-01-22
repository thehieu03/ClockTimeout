using System.Linq.Expressions;
using Common.Models;
using Order.Domain.Entities;

namespace Order.Domain.Repositories;

public interface IOrderRepository:IRepository<OrderEntity>
{

    #region Methods
    Task<OrderEntity?> GetByIdWithRelationshipAsync(Guid id,CancellationToken cancellationToken = default);
    Task<List<OrderEntity>> GetByCustomerWithRelationshipAsync(Guid customerId,CancellationToken cancellationToken = default);
    Task<OrderEntity?> GetByOrderNoAsync(string orderNo,CancellationToken cancellationToken = default);
    Task<List<OrderEntity>> SearchWithRelationshipAsync(Expression<Func<OrderEntity, bool>> predicate, PaginationRequest pagination,
        CancellationToken cancellationToken = default);
    Task<List<OrderEntity>> SearchWithRelationshipAsync(Expression<Func<OrderEntity, bool>> predicate, CancellationToken cancellationToken = default);
    #endregion

}