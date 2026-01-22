using Order.Domain.Enums;
using Order.Domain.Repositories;
using Order.Infrastructure.Data;

namespace Order.Infrastructure.Repositories;

public class OrderItemRepository(ApplicationDbContext context):Repository<OrderItemEntity>(context),IOrderItemRepository
{

    #region Implementation of IOrderItemRepository

    

    #endregion
}