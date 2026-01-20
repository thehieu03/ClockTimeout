using AutoMapper;

namespace Catalog.Application.Mappings;

/// <summary>
/// Main mapping profile that aggregates all sub-profiles.
/// This profile is kept for backward compatibility with DI registration.
/// For direct usage, use CatalogMapper.Mapper instead.
/// </summary>
public class CatalogMappingProfile : Profile
{
    public CatalogMappingProfile()
    {
        // All mappings are now in separate profile classes:
        // - ProductMappingProfile
        // - CategoryMappingProfile
        // - BrandMappingProfile
        // - ImageMappingProfile
        // These are registered via CatalogMapper static class or DI configuration
    }
}
