using AutoMapper;
using BuildingBlocks.CQRS;
using BuildingBlocks.Extensions;
using Common.Constants;
using Payment.Application.Dtos;
using Payment.Domain.Abstractions;

namespace Payment.Application.Features.Payment.Queries;

public record GetPaymentByOrderIdQuery(Guid OrderId) : IQuery<PaymentDto>;

public class GetPaymentByOrderIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    : IQueryHandler<GetPaymentByOrderIdQuery, PaymentDto>
{
    public async Task<PaymentDto> Handle(GetPaymentByOrderIdQuery query, CancellationToken cancellationToken)
    {
        var payment = await unitOfWork.Payments.GetByOrderIdAsync(query.OrderId, cancellationToken);

        if (payment is null)
            throw new NotFoundException(MessageCode.NotFound, query.OrderId);

        return mapper.Map<PaymentDto>(payment);
    }
}
