using AutoMapper;
using Payment.Application.Dtos;
using Payment.Domain.Entities;

namespace Payment.Application.Mappings;

public sealed class PaymentMappingProfile : Profile
{
    public PaymentMappingProfile()
    {
        CreateMap<PaymentEntity, PaymentDto>();
    }
}
