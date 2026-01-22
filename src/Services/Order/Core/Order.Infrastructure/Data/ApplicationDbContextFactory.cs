using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Order.Infrastructure.Data;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        // Use connection string for local development migrations
        // For docker: Server=sql-server,1433
        // For local: Server=localhost,1434 (mapped port from docker-compose)
        var connectionString = "Server=localhost,1434;Database=Order_Service;User Id=sa;Password=SqlServer123!;Encrypt=False;TrustServerCertificate=True;";

        // Create DbContextOptions
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
