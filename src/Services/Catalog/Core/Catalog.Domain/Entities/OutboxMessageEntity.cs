using Common.Constants;

namespace Catalog.Domain.Entities;

public sealed class OutboxMessageEntity : Entity<Guid>
{

    #region Fields, Properties and Indexers

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

    #region Factories

    public static OutboxMessageEntity Create(Guid id, string eventType, string content, DateTimeOffset occurredOnUtc)
    {
        return new OutboxMessageEntity()
        {
            Id = id,
            EventType = eventType,
            Content = content,
            OccurredOnUtc = occurredOnUtc,
            MaxAttemptCount = AppConstants.MaxAttempts,
            AttemptCount = 0
        };
    }
    public void Claim(DateTimeOffset claimedOnUtc) => ClaimedOnUtc = claimedOnUtc;
    public void SetRetryProperties(int attemptCount, int maxAttemptCount, DateTimeOffset? nextAttemptOnUtc, string? lastErrorMessage)
    {
        AttemptCount = attemptCount;
        MaxAttemptCount = maxAttemptCount;
        NextAttemptOnUtc = nextAttemptOnUtc;
        LastErrorMessage = lastErrorMessage;
    }
    public void RecordFailedAttempt(string errorMessage, DateTimeOffset currentTime)
    {
        IncreaseAttemptCount();
        if (AttemptCount >= MaxAttemptCount)
        {
            LastErrorMessage = $"Max attempt ({MaxAttemptCount}) reached. Last error: {errorMessage})";
            NextAttemptOnUtc = null;
        }
        else
        {
            var baseDelay = TimeSpan.FromSeconds(Math.Pow(2, AttemptCount - 1));
            var maxDelay = TimeSpan.FromMinutes(5);
            var jitter = TimeSpan.FromMilliseconds(Random.Shared.Next(0, 1000));
            var delay = TimeSpan.FromTicks(Math.Min(baseDelay.Ticks, maxDelay.Ticks)) + jitter;
            NextAttemptOnUtc = currentTime + delay;
            LastErrorMessage = errorMessage;
        }
    }
    private void IncreaseAttemptCount()
    {
        AttemptCount++;
    }
    public bool CanRetry(DateTimeOffset currentTime)
    {
        return AttemptCount < MaxAttemptCount && (NextAttemptOnUtc is null || currentTime >= NextAttemptOnUtc.Value);
    }
    public bool IsPermanentlyFailed() => AttemptCount >= MaxAttemptCount;

    #endregion

}