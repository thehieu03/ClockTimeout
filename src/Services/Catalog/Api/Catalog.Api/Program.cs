using Catalog.Application;
using Catalog.Infrastructure;

using Catalog.Api.Extensions;

namespace Catalog.Api;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddApplicationServices()
            .AddInfrastructureServices(builder.Configuration)
            .AddApiServices(builder.Configuration);
        var app = builder.Build();
        app.UseApi();
        app.UseInfrastructure();

        if (app.Environment.IsDevelopment())
        {
            await app.InitialiseDatabaseAsync();
        }

        app.Run();
    }
}
