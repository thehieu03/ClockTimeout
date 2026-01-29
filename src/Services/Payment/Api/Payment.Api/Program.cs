using Microsoft.EntityFrameworkCore;
using Payment.Infrastructure.Data;

namespace Payment.Api;

public partial class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        // Add services
        builder.Services.AddApiServices(builder.Configuration);
        // Add Swagger
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        var app = builder.Build();
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        using (var scope = app.Services.CreateScope())
        {
            var dbContext=scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            if (dbContext.Database.GetPendingMigrations().Any())
            {
                dbContext.Database.Migrate();
            }
        }
        app.UseHttpsRedirection();
        app.UseApi();
        app.Run();
    }
}

public partial class Program { }
