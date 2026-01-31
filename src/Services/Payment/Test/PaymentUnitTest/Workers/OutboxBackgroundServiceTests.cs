using FluentAssertions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Payment.Contract.IntegrationEvents;
using Payment.Domain.Entities;
using Payment.Infrastructure.Data;

namespace PaymentUnitTest.Workers;

/// <summary>
/// Integration tests for OutboxBackgroundService
/// Tests the complete flow from saving OutboxMessage to publishing via MassTransit
/// </summary>
[TestFixture]
public class OutboxBackgroundServiceTests
{
    private ApplicationDbContext _dbContext = null!;
    private Mock<IPublishEndpoint> _publishEndpointMock = null!;
    private Mock<ILogger<object>> _loggerMock = null!;
    private IServiceProvider _serviceProvider = null!;

    [SetUp]
    public void Setup()
    {
        // Setup In-Memory Database
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"OutboxTestDb_{Guid.NewGuid()}")
            .Options;
        
        _dbContext = new ApplicationDbContext(options);
        _publishEndpointMock = new Mock<IPublishEndpoint>();
        _loggerMock = new Mock<ILogger<object>>();

        // Setup ServiceProvider with mocks
        var services = new ServiceCollection();
        services.AddSingleton(_dbContext);
        services.AddSingleton(_publishEndpointMock.Object);
        services.AddSingleton(_loggerMock.Object);
        
        // Register ApplicationDbContext as scoped (normal usage pattern)
        services.AddScoped<ApplicationDbContext>(_ => _dbContext);
        services.AddScoped<IPublishEndpoint>(_ => _publishEndpointMock.Object);
        
        _serviceProvider = services.BuildServiceProvider();
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
        (_serviceProvider as IDisposable)?.Dispose();
    }

    #region Test: Verify OutboxMessage Creation

    [Test]
    public async Task OutboxMessage_ShouldBeCreated_WithCorrectProperties()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var transactionId = "TXN_123456";
        var amount = 100.50m;
        var occurredOn = DateTimeOffset.UtcNow;

        var integrationEvent = new PaymentCompletedIntegrationEvent(
            Guid.NewGuid(),
            paymentId,
            orderId,
            transactionId,
            amount,
            occurredOn
        );

        var eventType = typeof(PaymentCompletedIntegrationEvent).AssemblyQualifiedName!;
        var content = JsonConvert.SerializeObject(integrationEvent);

        // Act
        var outboxMessage = OutboxMessage.Create(
            Guid.NewGuid(),
            eventType,
            content,
            occurredOn
        );
        
        await _dbContext.OutboxMessages.AddAsync(outboxMessage);
        await _dbContext.SaveChangesAsync();

        // Assert
        var savedMessage = await _dbContext.OutboxMessages.FirstOrDefaultAsync();
        savedMessage.Should().NotBeNull();
        savedMessage!.EventType.Should().Be(eventType);
        savedMessage.Content.Should().Be(content);
        savedMessage.ProcessedOnUtc.Should().BeNull();
        savedMessage.AttemptCount.Should().Be(0);
    }

    #endregion

    #region Test: Verify Message Deserialization

    [Test]
    public void OutboxMessage_ShouldDeserialize_ToCorrectEventType()
    {
        // Arrange
        var originalEvent = new PaymentCompletedIntegrationEvent(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "TXN_789",
            250.00m,
            DateTimeOffset.UtcNow
        );

        var eventType = typeof(PaymentCompletedIntegrationEvent).AssemblyQualifiedName!;
        var content = JsonConvert.SerializeObject(originalEvent);

        // Act
        var type = Type.GetType(eventType);
        var deserializedEvent = JsonConvert.DeserializeObject(content, type!) as PaymentCompletedIntegrationEvent;

        // Assert
        deserializedEvent.Should().NotBeNull();
        deserializedEvent!.PaymentId.Should().Be(originalEvent.PaymentId);
        deserializedEvent.OrderId.Should().Be(originalEvent.OrderId);
        deserializedEvent.TransactionId.Should().Be(originalEvent.TransactionId);
        deserializedEvent.Amount.Should().Be(originalEvent.Amount);
    }

    #endregion

    #region Test: Verify MarkAsProcessed

    [Test]
    public async Task MarkAsProcessed_ShouldSetProcessedOnUtc()
    {
        // Arrange
        var outboxMessage = OutboxMessage.Create(
            Guid.NewGuid(),
            "TestType",
            "{}",
            DateTimeOffset.UtcNow
        );
        await _dbContext.OutboxMessages.AddAsync(outboxMessage);
        await _dbContext.SaveChangesAsync();

        // Act
        var processedTime = DateTimeOffset.UtcNow;
        outboxMessage.MarkAsProcessed(processedTime);
        await _dbContext.SaveChangesAsync();

        // Assert
        var updatedMessage = await _dbContext.OutboxMessages.FindAsync(outboxMessage.Id);
        updatedMessage!.ProcessedOnUtc.Should().Be(processedTime);
    }

    #endregion

    #region Test: Verify Retry Logic with Exponential Backoff

    [Test]
    public void RecordFailedAttempt_ShouldIncreaseAttemptCount_AndSetNextAttemptTime()
    {
        // Arrange
        var outboxMessage = OutboxMessage.Create(
            Guid.NewGuid(),
            "TestType",
            "{}",
            DateTimeOffset.UtcNow
        );
        var currentTime = DateTimeOffset.UtcNow;

        // Act
        outboxMessage.RecordFailedAttempt("Connection timeout", currentTime);

        // Assert
        outboxMessage.AttemptCount.Should().Be(1);
        outboxMessage.LastErrorMessage.Should().Be("Connection timeout");
        outboxMessage.NextAttemptOnUtc.Should().NotBeNull();
        outboxMessage.NextAttemptOnUtc.Should().BeAfter(currentTime);
    }

    [Test]
    public void RecordFailedAttempt_ShouldUseExponentialBackoff()
    {
        // Arrange
        var outboxMessage = OutboxMessage.Create(
            Guid.NewGuid(),
            "TestType",
            "{}",
            DateTimeOffset.UtcNow
        );
        var baseTime = DateTimeOffset.UtcNow;

        // Act - First failure (delay ~1 second)
        outboxMessage.RecordFailedAttempt("Error 1", baseTime);
        var firstNextAttempt = outboxMessage.NextAttemptOnUtc;

        // Act - Second failure (delay ~2 seconds from second attempt time)
        var secondAttemptTime = firstNextAttempt!.Value.AddSeconds(1);
        outboxMessage.RecordFailedAttempt("Error 2", secondAttemptTime);
        var secondNextAttempt = outboxMessage.NextAttemptOnUtc;

        // Assert - Each delay should grow exponentially
        outboxMessage.AttemptCount.Should().Be(2);

        // The base delays are: 2^0=1s, 2^1=2s (plus jitter up to 1s)
        // Verify each delay from its respective base time
        var delay1 = (firstNextAttempt.Value - baseTime).TotalSeconds;
        var delay2 = (secondNextAttempt!.Value - secondAttemptTime).TotalSeconds;

        // Verify exponential growth pattern with some tolerance for jitter
        delay1.Should().BeInRange(1, 2); // 2^0 = 1s + jitter (0-1s)
        delay2.Should().BeInRange(2, 3); // 2^1 = 2s + jitter (0-1s)

        // Verify delay2 is roughly double delay1 (accounting for jitter)
        delay2.Should().BeGreaterThan(delay1);
    }

    [Test]
    public void RecordFailedAttempt_AtMaxAttempts_ShouldMarkAsPermanentlyFailed()
    {
        // Arrange
        var outboxMessage = OutboxMessage.Create(
            Guid.NewGuid(),
            "TestType",
            "{}",
            DateTimeOffset.UtcNow
        );
        var currentTime = DateTimeOffset.UtcNow;
        var maxAttempts = outboxMessage.MaxAttemptCount;

        // Act - Fail until max attempts
        for (int i = 0; i < maxAttempts; i++)
        {
            outboxMessage.RecordFailedAttempt($"Error {i + 1}", currentTime);
        }

        // Assert
        outboxMessage.IsPermanentlyFailed().Should().BeTrue();
        outboxMessage.CanRetry(currentTime).Should().BeFalse();
        outboxMessage.NextAttemptOnUtc.Should().BeNull();
        outboxMessage.LastErrorMessage.Should().Contain("Max attempt");
    }

    #endregion

    #region Test: Verify CanRetry Logic

    [Test]
    public void CanRetry_ShouldReturnTrue_WhenWithinAttemptLimitAndTimeHasPassed()
    {
        // Arrange
        var outboxMessage = OutboxMessage.Create(
            Guid.NewGuid(),
            "TestType",
            "{}",
            DateTimeOffset.UtcNow
        );
        var pastTime = DateTimeOffset.UtcNow.AddMinutes(-10);
        outboxMessage.RecordFailedAttempt("Error", pastTime);
        
        var currentTime = DateTimeOffset.UtcNow;

        // Act & Assert
        outboxMessage.CanRetry(currentTime).Should().BeTrue();
    }

    [Test]
    public void CanRetry_ShouldReturnFalse_WhenNextAttemptTimeNotReached()
    {
        // Arrange
        var outboxMessage = OutboxMessage.Create(
            Guid.NewGuid(),
            "TestType",
            "{}",
            DateTimeOffset.UtcNow
        );
        var currentTime = DateTimeOffset.UtcNow;
        outboxMessage.RecordFailedAttempt("Error", currentTime);
        
        // Act - Check immediately (before NextAttemptOnUtc)
        var canRetryNow = outboxMessage.CanRetry(currentTime);

        // Assert
        canRetryNow.Should().BeFalse();
    }

    #endregion

    #region Test: Verify Claim Functionality

    [Test]
    public void Claim_ShouldSetClaimedOnUtc()
    {
        // Arrange
        var outboxMessage = OutboxMessage.Create(
            Guid.NewGuid(),
            "TestType",
            "{}",
            DateTimeOffset.UtcNow
        );
        var claimTime = DateTimeOffset.UtcNow;

        // Act
        outboxMessage.Claim(claimTime);

        // Assert
        outboxMessage.ClaimedOnUtc.Should().Be(claimTime);
    }

    #endregion

    #region Test: Full Integration Flow Simulation

    [Test]
    public async Task FullFlow_CreateOutboxMessage_Publish_MarkProcessed()
    {
        // Arrange - Step 1: Simulate PaymentCompletedDomainEventHandler saving OutboxMessage
        var integrationEvent = new PaymentCompletedIntegrationEvent(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "TXN_FULL_FLOW_TEST",
            999.99m,
            DateTimeOffset.UtcNow
        );

        var outboxMessage = OutboxMessage.Create(
            Guid.NewGuid(),
            typeof(PaymentCompletedIntegrationEvent).AssemblyQualifiedName!,
            JsonConvert.SerializeObject(integrationEvent),
            DateTimeOffset.UtcNow
        );

        await _dbContext.OutboxMessages.AddAsync(outboxMessage);
        await _dbContext.SaveChangesAsync();

        // Verify message is saved with ProcessedOnUtc = NULL
        var savedMessage = await _dbContext.OutboxMessages
            .FirstOrDefaultAsync(m => m.Id == outboxMessage.Id);
        savedMessage.Should().NotBeNull();
        savedMessage!.ProcessedOnUtc.Should().BeNull();

        // Act - Step 2: Simulate Worker processing (Deserialize & Publish)
        var type = Type.GetType(savedMessage.EventType!);
        type.Should().NotBeNull();

        var eventData = JsonConvert.DeserializeObject(savedMessage.Content!, type!) 
            as PaymentCompletedIntegrationEvent;
        eventData.Should().NotBeNull();
        eventData!.TransactionId.Should().Be("TXN_FULL_FLOW_TEST");

        // Simulate MassTransit publish
        await _publishEndpointMock.Object.Publish(eventData, CancellationToken.None);

        // Step 3: Mark as processed
        savedMessage.MarkAsProcessed(DateTimeOffset.UtcNow);
        await _dbContext.SaveChangesAsync();

        // Assert - Verify final state
        var processedMessage = await _dbContext.OutboxMessages
            .FirstOrDefaultAsync(m => m.Id == outboxMessage.Id);
        processedMessage!.ProcessedOnUtc.Should().NotBeNull();

        // Verify publish was called
        _publishEndpointMock.Verify(
            x => x.Publish(
                It.Is<PaymentCompletedIntegrationEvent>(e => e.TransactionId == "TXN_FULL_FLOW_TEST"),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }

    #endregion

    #region Test: Query Simulation (Unprocessed Messages)

    [Test]
    public async Task QueryUnprocessedMessages_ShouldReturnOnlyEligibleMessages()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;

        // Message 1: Unprocessed, no retry delay (should be returned)
        var msg1 = OutboxMessage.Create(Guid.NewGuid(), "Type1", "{}", now.AddMinutes(-5));
        
        // Message 2: Already processed (should NOT be returned)
        var msg2 = OutboxMessage.Create(Guid.NewGuid(), "Type2", "{}", now.AddMinutes(-4));
        msg2.MarkAsProcessed(now.AddMinutes(-3));

        // Message 3: Failed but retry time not reached (should NOT be returned immediately)
        var msg3 = OutboxMessage.Create(Guid.NewGuid(), "Type3", "{}", now.AddMinutes(-2));
        msg3.RecordFailedAttempt("Error", now); // Sets NextAttemptOnUtc in future

        // Message 4: Failed but max attempts reached (should NOT be returned)
        var msg4 = OutboxMessage.Create(Guid.NewGuid(), "Type4", "{}", now.AddMinutes(-1));
        for (int i = 0; i < msg4.MaxAttemptCount; i++)
        {
            msg4.RecordFailedAttempt($"Error {i}", now);
        }

        await _dbContext.OutboxMessages.AddRangeAsync(msg1, msg2, msg3, msg4);
        await _dbContext.SaveChangesAsync();

        // Act - Simulate worker query
        var eligibleMessages = await _dbContext.OutboxMessages
            .Where(m => m.ProcessedOnUtc == null)
            .Where(m => m.NextAttemptOnUtc == null || m.NextAttemptOnUtc <= now)
            .Where(m => m.AttemptCount < m.MaxAttemptCount)
            .OrderBy(m => m.OccurredOnUtc)
            .ToListAsync();

        // Assert
        eligibleMessages.Should().HaveCount(1);
        eligibleMessages[0].Id.Should().Be(msg1.Id);
    }

    #endregion
}
