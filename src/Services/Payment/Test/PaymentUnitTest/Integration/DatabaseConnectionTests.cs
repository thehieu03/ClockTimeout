using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using Payment.Infrastructure.Data;

namespace PaymentUnitTest.Integration;

[TestFixture]
[Category("Integration")]
public class DatabaseConnectionTests
{
    private IConfiguration _configuration;

    [SetUp]
    public void Setup()
    {
        var connectionString = "Server=localhost,1434;Database=Payment_Service;User Id=sa;Password=SqlServer123!;TrustServerCertificate=True;Encrypt=False";
        
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
        var masterConnectionString = connectionString.Replace("Database=Payment_Service", "Database=master");
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

        // 2. Try connecting to Payment_Service database
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
             Assert.Fail("Connected to 'master' successfully, but failed to connect to 'Payment_Service'. The database 'Payment_Service' likely does not exist. Did migrations run?");
        }
        
        canConnect.Should().BeTrue();
    }

    [Test]
    public async Task PaymentsTableExists()
    {
        // Arrange
        var connectionString = _configuration.GetConnectionString("Database");
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        using var context = new ApplicationDbContext(optionsBuilder.Options);

        // Act & Assert
        var canConnect = await context.Database.CanConnectAsync();
        canConnect.Should().BeTrue("Database connection should be successful");

        // Check if the Payments table exists by querying it
        var paymentsCount = await context.Payments.CountAsync();
        paymentsCount.Should().BeGreaterThanOrEqualTo(0, "Payments table should exist and be queryable");
    }
}
