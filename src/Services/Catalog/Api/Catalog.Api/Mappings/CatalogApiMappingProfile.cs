using System.Text.RegularExpressions;
using AutoMapper;
using Catalog.Api.Models;
using Catalog.Application.Dtos;
using Catalog.Application.Dtos.Brands;
using Catalog.Application.Dtos.Categories;
using Catalog.Application.Dtos.Products;

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

        // UpdateProductRequest => UpdateProductDto
        CreateMap<UpdateProductRequest, UpdateProductDto>()
            .ForMember(dest => dest.CategoryIds, otp => otp.Ignore());

        // CreateCategoryRequest => CreateCategoryDto
        CreateMap<CreateCategoryRequest, CreateCategoryDto>();

        // UpdateCategoryRequest => UpdateCategoryDto
        CreateMap<UpdateCategoryRequest, UpdateCategoryDto>();

        // CreateBrandRequest => CreateBrandDto
        CreateMap<CreateBrandRequest, CreateBrandDto>();

        // UpdateBrandRequest => UpdateBrandDto
        CreateMap<UpdateBrandRequest, UpdateBrandDto>();

    }
    #endregion
}