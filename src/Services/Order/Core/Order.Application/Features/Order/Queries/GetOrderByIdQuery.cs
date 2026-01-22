using AutoMapper;
using BuildingBlocks.CQRS;
using BuildingBlocks.Extensions;
using Common.Constants;
using Order.Application.Dtos.Orders;
using Order.Domain.Abstractions;

namespace Order.Application.Features.Order.Queries;

public record GetOrderByIdQuery(Guid OrderId) : IQuery<OrderDto>;


public class GetOrderByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    : IQueryHandler<GetOrderByIdQuery, OrderDto>
{
    public async Task<OrderDto> Handle(GetOrderByIdQuery query, CancellationToken cancellationToken)
    {
        // 1. Get Order from Repository with relationships (Customer, ShippingAddress, OrderItems)
        var order = await unitOfWork.Orders.GetByIdWithRelationshipAsync(query.OrderId, cancellationToken);

        // 2. Validate existence
        if (order == null)
        {
            throw new NotFoundException(MessageCode.OrderNotFound, query.OrderId);
        }

        // 3. Map to DTO
        var orderDto = mapper.Map<OrderDto>(order);

        return orderDto;
    }
}
