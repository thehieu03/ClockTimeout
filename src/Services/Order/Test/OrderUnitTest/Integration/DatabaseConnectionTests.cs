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
    private IConfiguration _configuration;

    [SetUp]
    public void Setup()
    {
        // Require connection string. 
        // Strategy: Try to read from Api appsettings if possible, or hardcode for test.
        // For local development robustness, I will hardcode the known working connection string from our previous fixes 
        // but also allow override via Environment Variable.
        
        var connectionString = "Server=localhost,1434;Database=OrderDb;User Id=sa;Password=SqlServer123!;TrustServerCertificate=True;Encrypt=False";
        
        // Build configuration just to simulate realistic setup if needed, but for simple connection test:
        var myConfiguration = new Dictionary<string, string>
        {
            {"ConnectionStrings:Database", connectionString}
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(myConfiguration)
            .Build();
    }

    [Test]
    public async Task CanConnectToDatabase()
    {
        // Arrange
        var connectionString = _configuration.GetConnectionString("Database");
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        using var context = new ApplicationDbContext(optionsBuilder.Options);

        // Act
        // 1. Try connecting to Master to verify credentials
        var masterConnectionString = connectionString.Replace("Database=OrderDb", "Database=master");
        var masterOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(masterConnectionString)
            .Options;
            
        using var masterContext = new ApplicationDbContext(masterOptions);
        bool canConnectMaster = false;
        try {
             canConnectMaster = await masterContext.Database.CanConnectAsync();
        } catch {}

        if (!canConnectMaster)
        {
             Assert.Fail("Could not connect to 'master' database. Credentials or SQL Server might be wrong/down.");
        }

        // Try to apply migrations if database doesn't exist
        try 
        {
            await context.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            Assert.Fail($"Migration failed: {ex.Message} -> Inner: {ex.InnerException?.Message}");
        }

        // 2. Try connecting to OrderDb again
        bool canConnect = false;
        try 
        {
            canConnect = await context.Database.CanConnectAsync();
        }
        catch (Exception ex)
        {
             Assert.Fail($"CanConnectAsync threw exception: {ex.Message} -> Inner: {ex.InnerException?.Message}");
        }

        if (!canConnect)
        {
             Assert.Fail("Connected to 'master' successfully, but failed to connect to 'OrderDb'. The database 'OrderDb' likely does not exist. Did migrations run?");
        }
        
        canConnect.Should().BeTrue();
    }
}
