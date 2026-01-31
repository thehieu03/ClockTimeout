using Common.Configurations;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Payment.Infrastructure;
using Payment.Infrastructure.Data;
using Payment.Worker.Jobs;
using Payment.Worker.Workers;

var builder = Host.CreateApplicationBuilder(args);

// Database Configuration
// Register Infrastructure Services (DbContext, Gateways, Repositories)
builder.Services.AddInfrastructureServices(builder.Configuration);

// MassTransit + RabbitMQ Configuration
builder.Services.AddMassTransit(bus =>
{
    bus.UsingRabbitMq((context, cfg) =>
    {
        var host = builder.Configuration[$"{MessageBrokerCfg.Section}:{MessageBrokerCfg.Host}"];
        var username = builder.Configuration[$"{MessageBrokerCfg.Section}:{MessageBrokerCfg.UserName}"];
        var password = builder.Configuration[$"{MessageBrokerCfg.Section}:{MessageBrokerCfg.Password}"];
        
        cfg.Host(host, "/", h =>
        {
            h.Username(username);
            h.Password(password);
        });
        cfg.ConfigureEndpoints(context);
    });
});

// Background Services
builder.Services.AddHostedService<OutboxBackgroundService>();
builder.Services.AddHostedService<ReconcilePaymentBackgroundService>();

var host = builder.Build();
host.Run();