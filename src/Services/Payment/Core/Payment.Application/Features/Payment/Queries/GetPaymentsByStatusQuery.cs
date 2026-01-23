using AutoMapper;
using BuildingBlocks.CQRS;
using Payment.Application.Dtos;
using Payment.Domain.Abstractions;
using Payment.Domain.Enums;

namespace Payment.Application.Features.Payment.Queries;

public record GetPaymentsByStatusQuery(PaymentStatus Status) : IQuery<IReadOnlyList<PaymentDto>>;

public class GetPaymentsByStatusQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    : IQueryHandler<GetPaymentsByStatusQuery, IReadOnlyList<PaymentDto>>
{
    public async Task<IReadOnlyList<PaymentDto>> Handle(GetPaymentsByStatusQuery query, CancellationToken cancellationToken)
    {
        var payments = await unitOfWork.Payments.GetByStatusAsync(query.Status, cancellationToken);

        return mapper.Map<IReadOnlyList<PaymentDto>>(payments);
    }
}
