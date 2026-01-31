using MassTransit;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Payment.Infrastructure.Data;

namespace Payment.Worker.Workers;

public class OutboxBackgroundService(IServiceProvider serviceProvider,ILogger<OutboxBackgroundService> logger):BackgroundService
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly ILogger<OutboxBackgroundService> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OutboxBackgroundService started");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxBatchAsync(stoppingToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error processing outbox batch");
            }
            await Task.Delay(1000, stoppingToken);
        }
    }
    
    private async Task ProcessOutboxBatchAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
        
        // ExecutionStrategy để handle retry kết nối nếu Db chập 
        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            // Bắt đầu Transaction
            using var transaction = await dbContext.Database.BeginTransactionAsync(stoppingToken);
            try
            {
                var currentTime = DateTimeOffset.UtcNow;
                
                // 1. Fetch & Lock Messages
                // "FOR UPDATE SKIP LOCKED": Lock các dòng tìm thấy, nếu dòng đang bị lock bởi transaction khác thì bỏ qua.
                // Điều này giúp chạy nhiều Worker song song mà không bị trùng lặp.
                var messages = await dbContext.OutboxMessages
                    .FromSqlRaw(@"
                        SELECT * FROM ""OutboxMessages"" 
                        WHERE ""ProcessedOnUtc"" IS NULL 
                        AND (""NextAttemptOnUtc"" IS NULL OR ""NextAttemptOnUtc"" <= {0})
                        AND ""AttemptCount"" < ""MaxAttemptCount""
                        ORDER BY ""OccurredOnUtc"" 
                        LIMIT 20 
                        FOR UPDATE SKIP LOCKED", currentTime)
                    .ToListAsync(stoppingToken);

                if (!messages.Any())
                {
                    // Không có tin nhắn, commit (không làm gì) & return
                    await transaction.CommitAsync(stoppingToken);
                    return;
                }

                _logger.LogInformation("Found {Count} unprocessed messages to process", messages.Count);

                foreach (var message in messages)
                {
                    try
                    {
                        // 2. Claim message (đánh dấu đang xử lý)
                        message.Claim(currentTime);
                        
                        // 3. Deserialize Event
                        var type = Type.GetType(message.EventType!);
                        if (type == null)
                        {
                            var error = $"Type '{message.EventType}' not found in Worker Assembly";
                            _logger.LogWarning("Type not found for Message {Id}: {Type}", message.Id, message.EventType);
                            message.RecordFailedAttempt(error, currentTime);
                            continue;
                        }
                        var eventData = JsonConvert.DeserializeObject(message.Content!, type);
                        if (eventData == null)
                        {
                            var error = "Failed to deserialize event content";
                            _logger.LogWarning("Deserialization failed for Message {Id}", message.Id);
                            message.RecordFailedAttempt(error, currentTime);
                            continue;
                        }

                        // 4. Publish to RabbitMQ via MassTransit
                        await publishEndpoint.Publish(eventData, stoppingToken);
                        _logger.LogInformation("Successfully published: {Type} - MessageId: {Id}", type.Name, message.Id);

                        // 5. Mark as processed
                        message.MarkAsProcessed(currentTime);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to publish message {Id} (Attempt {Attempt}/{MaxAttempt})", 
                            message.Id, message.AttemptCount + 1, message.MaxAttemptCount);
                        
                        message.RecordFailedAttempt(ex.Message, currentTime);
                        
                        // Không throw ở đây để tiếp tục xử lý các message khác trong batch
                    }
                }

                // 6. Save Changes & Commit Transaction
                await dbContext.SaveChangesAsync(stoppingToken);
                await transaction.CommitAsync(stoppingToken);
                
                _logger.LogInformation("Batch processing completed. Processed/Failed: {Count} messages", messages.Count);
            }
            catch (Exception ex)
            {
                // Rollback nếu có lỗi nghiêm trọng cấp DB
                _logger.LogError(ex, "Critical error in batch processing, rolling back transaction");
                await transaction.RollbackAsync(stoppingToken);
                throw;
            }
        });
    }
}
