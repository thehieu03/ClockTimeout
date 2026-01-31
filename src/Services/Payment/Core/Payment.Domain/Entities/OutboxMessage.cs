using BuildingBlocks.Abstractions;
using Common.Constants;

namespace Payment.Domain.Entities;

/// <summary>
/// Entity representing an outbox message for reliable event publishing
/// </summary>
public sealed class OutboxMessage : Entity<Guid>
{
    #region Properties

    public string? EventType { get; set; }
    public string? Content { get; set; }
    public DateTimeOffset OccurredOnUtc { get; set; }
    public DateTimeOffset? ProcessedOnUtc { get; set; }
    public string? LastErrorMessage { get; set; }
    public DateTimeOffset? ClaimedOnUtc { get; set; }
    public int AttemptCount { get; set; }
    public int MaxAttemptCount { get; set; }
    public DateTimeOffset? NextAttemptOnUtc { get; set; }

    #endregion

    #region Factory Methods

    public static OutboxMessage Create(Guid id, string eventType, string content, DateTimeOffset occurredOnUtc)
    {
        return new OutboxMessage
        {
            Id = id,
            EventType = eventType,
            Content = content,
            OccurredOnUtc = occurredOnUtc,
            MaxAttemptCount = AppConstants.MaxAttempts,
            AttemptCount = 0
        };
    }

    public void MarkAsProcessed(DateTimeOffset processedOnUtc)
    {
        ProcessedOnUtc = processedOnUtc;
    }

    public void Claim(DateTimeOffset claimedOnUtc) => ClaimedOnUtc = claimedOnUtc;

    public void RecordFailedAttempt(string errorMessage, DateTimeOffset currentTime)
    {
        AttemptCount++;
        if (AttemptCount >= MaxAttemptCount)
        {
            LastErrorMessage = $"Max attempt ({MaxAttemptCount}) reached. Last error: {errorMessage}";
            NextAttemptOnUtc = null;
        }
        else
        {
            // Exponential backoff with jitter
            var baseDelay = TimeSpan.FromSeconds(Math.Pow(2, AttemptCount - 1));
            var maxDelay = TimeSpan.FromMinutes(5);
            var jitter = TimeSpan.FromMilliseconds(Random.Shared.Next(0, 1000));
            var delay = TimeSpan.FromTicks(Math.Min(baseDelay.Ticks, maxDelay.Ticks)) + jitter;
            NextAttemptOnUtc = currentTime + delay;
            LastErrorMessage = errorMessage;
        }
    }

    public bool CanRetry(DateTimeOffset currentTime)
    {
        return AttemptCount < MaxAttemptCount && (NextAttemptOnUtc is null || currentTime >= NextAttemptOnUtc.Value);
    }

    public bool IsPermanentlyFailed() => AttemptCount >= MaxAttemptCount;

    #endregion
}
