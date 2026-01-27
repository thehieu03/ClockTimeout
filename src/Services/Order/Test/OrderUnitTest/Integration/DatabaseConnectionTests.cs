using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using Order.Infrastructure.Data;

namespace OrderUnitTest.Integration;

[TestFixture]
[Category("Integration")]
public class DatabaseConnectionTests
{
    private IConfiguration _configuration = null!;
    private string _connectionString = null!;

    [SetUp]
    public void Setup()
    {
        // PostgreSQL connection string for local Docker container
        _connectionString = "Host=localhost;Port=5433;Database=OrderDb;Username=postgres;Password=postgres123";
        
        var myConfiguration = new Dictionary<string, string?>
        {
            {"ConnectionStrings:Database", _connectionString}
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(myConfiguration)
            .Build();
    }

    [Test]
    public async Task CanConnectToDatabase()
    {
        // Arrange - First verify PostgreSQL server is running
        var postgresConnectionString = _connectionString.Replace("Database=OrderDb", "Database=postgres");
        var postgresOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(postgresConnectionString)
            .Options;
            
        using var postgresContext = new ApplicationDbContext(postgresOptions);
        
        bool canConnectPostgres = false;
        try 
        {
            canConnectPostgres = await postgresContext.Database.CanConnectAsync();
        } 
        catch (Exception ex)
        {
            Assert.Ignore($"PostgreSQL server is not available. Skipping integration test. Error: {ex.Message}");
            return;
        }

        if (!canConnectPostgres)
        {
            Assert.Ignore("Could not connect to PostgreSQL 'postgres' database. Skipping integration test.");
            return;
        }

        // Act - Create and migrate the OrderDb database
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseNpgsql(_connectionString);

        using var context = new ApplicationDbContext(optionsBuilder.Options);

        try 
        {
            await context.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            Assert.Fail($"Migration failed: {ex.Message} -> Inner: {ex.InnerException?.Message}");
        }

        // Assert - Verify connection to OrderDb
        bool canConnect = false;
        try 
        {
            canConnect = await context.Database.CanConnectAsync();
        }
        catch (Exception ex)
        {
            Assert.Fail($"CanConnectAsync threw exception: {ex.Message} -> Inner: {ex.InnerException?.Message}");
        }

        canConnect.Should().BeTrue("Should be able to connect to OrderDb after migration");
    }
}
