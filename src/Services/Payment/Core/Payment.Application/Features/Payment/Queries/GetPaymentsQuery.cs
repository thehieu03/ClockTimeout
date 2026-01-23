using AutoMapper;
using BuildingBlocks.CQRS;
using Payment.Application.Dtos;
using Payment.Domain.Abstractions;

namespace Payment.Application.Features.Payment.Queries;

public record GetPaymentsQuery : IQuery<IReadOnlyList<PaymentDto>>;

public class GetPaymentsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    : IQueryHandler<GetPaymentsQuery, IReadOnlyList<PaymentDto>>
{
    public async Task<IReadOnlyList<PaymentDto>> Handle(GetPaymentsQuery query, CancellationToken cancellationToken)
    {
        var payments = await unitOfWork.Payments.GetAllAsync(cancellationToken);

        return mapper.Map<IReadOnlyList<PaymentDto>>(payments);
    }
}
