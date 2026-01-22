using Order.Domain.Entities;
using Order.Domain.Repositories;
using Order.Infrastructure.Data;

namespace Order.Infrastructure.Repositories;

public class OutboxMessageRepository(ApplicationDbContext context):Repository<OutBoxMessageEntity>(context),IOutboxMessageRepository
{
    
}