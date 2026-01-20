using AutoMapper;
using Catalog.Application.Dtos.Brands;
using Catalog.Application.Dtos.Categories;
using Catalog.Application.Dtos.Products;
using Catalog.Application.Models.Filters;
using Catalog.Application.Models.Results;
using Catalog.Domain.Entities;
using Catalog.Domain.Enums;
using System.ComponentModel;
using System.Reflection;

namespace Catalog.Application.Mappings;

public class CatalogMappingProfile : Profile
{
    public CatalogMappingProfile()
    {
        CreateProductMappings();
        CreateCategoryMappings();
        CreateBrandMappings();
        CreateImageMappings();
        CreateResultMappings();
    }

    private void CreateResultMappings()
    {
        // ProductEntity -> GetPublishProductByIdResult
        CreateMap<ProductEntity, GetPublishProductByIdResult>()
            .ForMember(dest => dest.Product, opt => opt.MapFrom(src => src));
    }

    private void CreateImageMappings()
    {
        // ProductImageEntity -> ProductImageDto
        CreateMap<ProductImageEntity, ProductImageDto>();
        // ProductImageEntity -> PublishProductImageDto
        CreateMap<ProductImageEntity, PublishProductImageDto>()
            .ForMember(dest => dest.FileName, opt => opt.MapFrom(src => src.FileName))
            .ForMember(dest=>dest.PublicURL,opt=>opt.MapFrom(src=>src.PublicURL));
        // UploadFileBytes -> ProductImageEntity
        CreateMap<UploadFileBytes, ProductImageEntity>()
            .ForMember(dest => dest.FileId, opt => opt.Ignore())
            .ForMember(dest => dest.OriginalFileName, opt => opt.MapFrom(src => src.FileName))
            .ForMember(dest => dest.FileName, opt => opt.Ignore())
            .ForMember(dest => dest.PublicURL, opt => opt.Ignore());
        // UploadFileResult -> ProductImageEntity
        CreateMap<UploadFileBytes, ProductImageEntity>()
            .ReverseMap();
    }

    private void CreateBrandMappings()
    {
        // BrandEntity -> BrandDto
        CreateMap<BrandEntity,BrandDto>();

    }

    private void CreateCategoryMappings()
    {
        // CategoryEntity -> CategoryDto
        CreateMap<CategoryEntity, CategoryDto>();
    }

    private void CreateProductMappings()
    {
        // ProductEntity -> ProductDto
        CreateMap<ProductEntity, Catalog.Application.Dtos.Products.ProductDto>()
            .ForMember(dest => dest.DisplayStatus, opt => opt.MapFrom(src=>src.Status.GetDescription()));
        // ProductEntity -> PublishProductDto
        CreateMap<ProductEntity, PublishProductDto>()
            .ForMember(dest => dest.DisplayStatus, opt => opt.MapFrom(src => src.Status.GetDescription()));
        // ProductEntity -> GetProductByIdResult
        CreateMap<ProductEntity, GetProductByIdResult>()
            .ForMember(dest => dest.Product, opt => opt.MapFrom(src => src));

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
