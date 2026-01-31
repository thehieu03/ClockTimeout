using Common.Configurations;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Payment.Infrastructure.Data;
using Payment.Worker.Workers;

var builder = Host.CreateApplicationBuilder(args);

// Database Configuration
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString(ConnectionStringsCfg.Database));
});

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

var host = builder.Build();
host.Run();