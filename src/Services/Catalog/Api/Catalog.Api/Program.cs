using Catalog.Application;
using Catalog.Infrastructure;

namespace Catalog.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddApplicationServices()
            .AddInfrastructureServices(builder.Configuration)
            .AddApiServices(builder.Configuration);
        var app = builder.Build();
        app.UseApi();
        app.UseInfrastructure();

        app.Run();
    }
}
