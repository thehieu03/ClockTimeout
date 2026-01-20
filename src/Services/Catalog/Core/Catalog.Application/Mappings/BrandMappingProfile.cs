using AutoMapper;
using Catalog.Application.Dtos.Brands;
using Catalog.Domain.Entities;

namespace Catalog.Application.Mappings;

public class BrandMappingProfile : Profile
{
    public BrandMappingProfile()
    {
        // BrandEntity => BrandDto
        CreateMap<BrandEntity, BrandDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Slug, opt => opt.MapFrom(src => src.Slug));
    }
}
