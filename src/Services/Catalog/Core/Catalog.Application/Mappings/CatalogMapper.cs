using AutoMapper;

namespace Catalog.Application.Mappings;

public static class CatalogMapper
{
    private static readonly Lazy<IMapper> _mapper = new Lazy<IMapper>(() =>
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.ShouldMapProperty = p => p.GetMethod!.IsPublic || p.GetMethod.IsAssembly;
            cfg.AddProfile<ProductMappingProfile>();
            cfg.AddProfile<CategoryMappingProfile>();
            cfg.AddProfile<BrandMappingProfile>();
            cfg.AddProfile<ImageMappingProfile>();
        });

        return config.CreateMapper();
    });

    public static IMapper Mapper => _mapper.Value;
}
