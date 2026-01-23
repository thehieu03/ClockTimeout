using AutoMapper;
using BuildingBlocks.CQRS;
using Order.Application.Dtos.Orders;
using Order.Domain.Abstractions;

namespace Order.Application.Features.Order.Queries;

public record GetOrdersQuery() : IQuery<List<OrderDto>>;

public class GetOrdersQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    : IQueryHandler<GetOrdersQuery, List<OrderDto>>
{
    public async Task<List<OrderDto>> Handle(GetOrdersQuery query, CancellationToken cancellationToken)
    {
        // Get all orders with relationships
        var orders = await unitOfWork.Orders.SearchWithRelationshipAsync(x => true, cancellationToken);
        
        return mapper.Map<List<OrderDto>>(orders);
    }
}
