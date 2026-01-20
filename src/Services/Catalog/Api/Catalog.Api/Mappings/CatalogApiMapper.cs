using AutoMapper;
using Catalog.Application.Mappings;

namespace Catalog.Api.Mappings;

public static class CatalogApiMapper
{
    private static readonly Lazy<IMapper> _mapper = new Lazy<IMapper>(() =>
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.ShouldMapProperty = p => p.GetMethod!.IsPublic || p.GetMethod.IsAssembly;

            // Add Application layer profiles
            cfg.AddProfile<ProductMappingProfile>();
            cfg.AddProfile<CategoryMappingProfile>();
            cfg.AddProfile<BrandMappingProfile>();
            cfg.AddProfile<ImageMappingProfile>();

            // Add API layer profiles
            cfg.AddProfile<CatalogApiMappingProfile>();
        });

        return config.CreateMapper();
    });

    public static IMapper Mapper => _mapper.Value;
}
