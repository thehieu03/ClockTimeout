using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Order.Domain.Abstractions;
using Order.Domain.Repositories;
using Order.Infrastructure.Data;
using Order.Infrastructure.Repositories;

namespace Order.Infrastructure;

public static class DependencyInjection
{

    #region Methods

    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services,IConfiguration configuration)
    {
        var connectionString=configuration.GetConnectionString("Database");
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
        });
        // Register Repositories, Unit of Work, etc.
        services.AddScoped<IOrderRepository,OrderRepository>();
        services.AddScoped<IOrderItemRepository,OrderItemRepository>();
        services.AddScoped<IInboxMessageRepository,InboxMessageRepository>();
        services.AddScoped<IOutboxMessageRepository,OutboxMessageRepository>();
        services.AddScoped<IUnitOfWork,UnitOfWork.UnitOfWork>();
        return services;
    }
    

    #endregion
}