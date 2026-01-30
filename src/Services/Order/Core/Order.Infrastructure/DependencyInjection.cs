using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Order.Domain.Abstractions;
using Order.Domain.Repositories;
using Order.Infrastructure.Data;
using Order.Infrastructure.Repositories;
using Order.Application.Services;
using Order.Infrastructure.Services;
using Catalog.Grpc;

namespace Order.Infrastructure;

public static class DependencyInjection
{

    #region Methods

    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services,IConfiguration configuration)
    {
        var connectionString=configuration.GetConnectionString("Database");
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });
        // Register Repositories, Unit of Work, etc.
        services.AddScoped<IOrderRepository,OrderRepository>();
        services.AddScoped<IOrderItemRepository,OrderItemRepository>();
        services.AddScoped<IInboxMessageRepository,InboxMessageRepository>();
        services.AddScoped<IOutboxMessageRepository,OutboxMessageRepository>();
        services.AddScoped<IUnitOfWork,UnitOfWork.UnitOfWork>();

        // Register gRPC services
        var catalogGrpcUrl = configuration["GrpcClients:Catalog:Url"] ?? "http://catalog-api:8080";
        services.AddGrpcClient<CatalogGrpc.CatalogGrpcClient>(options =>
        {
            options.Address = new Uri(catalogGrpcUrl);
        });
        services.AddScoped<ICatalogGrpcService, CatalogGrpcService>();

        return services;
    }
    

    #endregion
}