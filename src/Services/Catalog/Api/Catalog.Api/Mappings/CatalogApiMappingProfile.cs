using System.Text.RegularExpressions;
using AutoMapper;
using Catalog.Api.Models;
using Catalog.Application.Dtos;

namespace Catalog.Api.Mappings;

public sealed class CatalogApiMappingProfile : Profile
{

    #region ctor
    public CatalogApiMappingProfile()
    {
        // CreateProductRequest => CreateProductDto
        CreateMap<CreateProductRequest, CreateProductDto>()
            .ForMember(dest => dest.UploadImages, otp => otp.Ignore())
            .ForMember(dest => dest.UploadThumbnail, otp => otp.Ignore());// Files are handled separately
        
        // CreateProductRequest => CreateProductDto
        CreateMap<UpdateProductRequest, CreateProductDto>()
            .ForMember(dest => dest.UploadImages, otp => otp.Ignore())
            .ForMember(dest => dest.UploadThumbnail, otp => otp.Ignore());// Files are handled separately
        
    }
    #endregion
}