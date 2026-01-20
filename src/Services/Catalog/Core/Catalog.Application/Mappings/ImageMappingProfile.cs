using AutoMapper;
using Catalog.Application.Dtos.Products;
using Catalog.Domain.Entities;

namespace Catalog.Application.Mappings;

public class ImageMappingProfile : Profile
{
    public ImageMappingProfile()
    {
        // ProductImageEntity => ProductImageDto
        CreateMap<ProductImageEntity, ProductImageDto>()
            .ForMember(dest => dest.FileId, opt => opt.MapFrom(src => src.FileId))
            .ForMember(dest => dest.OriginalFileName, opt => opt.MapFrom(src => src.OriginalFileName))
            .ForMember(dest => dest.FileName, opt => opt.MapFrom(src => src.FileName))
            .ForMember(dest => dest.PublicURL, opt => opt.MapFrom(src => src.PublicURL));

        // ProductImageEntity => PublishProductImageDto
        CreateMap<ProductImageEntity, PublishProductImageDto>()
            .ForMember(dest => dest.FileName, opt => opt.MapFrom(src => src.FileName))
            .ForMember(dest => dest.PublicURL, opt => opt.MapFrom(src => src.PublicURL));
    }
}
