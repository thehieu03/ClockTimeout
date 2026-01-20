using AutoMapper;
using Catalog.Application.Dtos.Products;
using Catalog.Domain.Entities;
using Catalog.Domain.Enums;
using System.ComponentModel;
using System.Reflection;

namespace Catalog.Application.Mappings;

public class ProductMappingProfile : Profile
{
    public ProductMappingProfile()
    {
        // ProductEntity => ProductInfoDto
        CreateMap<ProductEntity, ProductInfoDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Sku, opt => opt.MapFrom(src => src.Sku))
            .ForMember(dest => dest.ShortDescription, opt => opt.MapFrom(src => src.ShortDescription))
            .ForMember(dest => dest.LongDescription, opt => opt.MapFrom(src => src.LongDescription))
            .ForMember(dest => dest.Slug, opt => opt.MapFrom(src => src.Slug))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
            .ForMember(dest => dest.SalePrice, opt => opt.MapFrom(src => src.SalePrice))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.DisplayStatus, opt => opt.MapFrom(src => GetStatusDescription(src.Status)))
            .ForMember(dest => dest.Colors, opt => opt.MapFrom(src => src.Colors))
            .ForMember(dest => dest.Sizes, opt => opt.MapFrom(src => src.Sizes))
            .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.Tags))
            .ForMember(dest => dest.Published, opt => opt.MapFrom(src => src.Published))
            .ForMember(dest => dest.Featured, opt => opt.MapFrom(src => src.Featured))
            .ForMember(dest => dest.SEOTitle, opt => opt.MapFrom(src => src.SEOTitle))
            .ForMember(dest => dest.SEODescription, opt => opt.MapFrom(src => src.SEODescription))
            .ForMember(dest => dest.Unit, opt => opt.MapFrom(src => src.Unit))
            .ForMember(dest => dest.Weight, opt => opt.MapFrom(src => src.Weight))
            .ForMember(dest => dest.CategoryNames, opt => opt.Ignore()) // Set in query handler
            .ForMember(dest => dest.BrandName, opt => opt.Ignore()); // Set in query handler

        // ProductEntity => PublishProductDto
        CreateMap<ProductEntity, PublishProductDto>()
            .IncludeBase<ProductEntity, ProductInfoDto>()
            .ForMember(dest => dest.Thumbnail, opt => opt.MapFrom(src => src.Thumbnail))
            .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Images));
    }

    private static string GetStatusDescription(ProductStatus status)
    {
        var field = status.GetType().GetField(status.ToString());
        if (field == null) return status.ToString();

        var attribute = field.GetCustomAttributes(typeof(DescriptionAttribute), false)
            .FirstOrDefault() as DescriptionAttribute;

        return attribute?.Description ?? status.ToString();
    }
}
