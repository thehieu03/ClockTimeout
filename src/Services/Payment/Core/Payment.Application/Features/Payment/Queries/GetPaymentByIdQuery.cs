using AutoMapper;
using BuildingBlocks.CQRS;
using BuildingBlocks.Extensions;
using Common.Constants;
using Payment.Application.Dtos;
using Payment.Domain.Abstractions;

namespace Payment.Application.Features.Payment.Queries;

public record GetPaymentByIdQuery(Guid PaymentId) : IQuery<PaymentDto>;

public class GetPaymentByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    : IQueryHandler<GetPaymentByIdQuery, PaymentDto>
{
    public async Task<PaymentDto> Handle(GetPaymentByIdQuery query, CancellationToken cancellationToken)
    {
        var payment = await unitOfWork.Payments.GetByIdAsync(query.PaymentId, cancellationToken);

        if (payment is null)
            throw new NotFoundException(MessageCode.NotFound, query.PaymentId);

        return mapper.Map<PaymentDto>(payment);
    }
}
