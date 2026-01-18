namespace Catalog.Api;

public static class DependencyInjection
{

    #region  Methods

    public static IServiceCollection AddApiServices(this IServiceCollection services,IConfiguration cfg)
    {
        // services.AddExceptionHandler<Customer>()
        return services;
    }
    

    #endregion
}