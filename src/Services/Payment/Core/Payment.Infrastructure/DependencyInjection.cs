using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Payment.Application.Gateways;
using Payment.Domain.Abstractions;
using Payment.Domain.Repositories;
using Payment.Infrastructure.Configurations;
using Payment.Infrastructure.Data;
using Payment.Infrastructure.Gateways;
using Payment.Infrastructure.Gateways.Momo;
using Payment.Infrastructure.Gateways.VnPay;
using Payment.Infrastructure.Repositories;

namespace Payment.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database");
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(connectionString, builder => builder.EnableRetryOnFailure());
        });
        // Bind settings
        services.Configure<MomoSettings>(configuration.GetSection(MomoSettings.SectionName));
        services.AddHttpClient<MomoPaymentGateway>();
        services.AddScoped<IPaymentGateway, MomoPaymentGateway>();
        // Register Repositories
        services.AddScoped<IPaymentRepository, PaymentRepository>();

        // Register Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register VnPay Settings
        var vnpaySettings = configuration.GetSection(VnPaySettings.SectionName).Get<VnPaySettings>() 
            ?? new VnPaySettings();
        services.AddSingleton(vnpaySettings);

        // Register HttpClient for VnPay
        services.AddHttpClient<VnPayPaymentGateway>();

        // Register Payment Gateways
        services.AddScoped<IPaymentGateway, MockPaymentGateway>();
        services.AddScoped<IPaymentGateway, CodPaymentGateway>();
        services.AddScoped<IPaymentGateway, VnPayPaymentGateway>();

        // Register Gateway Factory
        services.AddScoped<IPaymentGatewayFactory, PaymentGatewayFactory>();

        // Register VietQR Settings
        services.Configure<VietQRSettings>(configuration.GetSection(VietQRSettings.SectionName));

        return services;
    }
}
