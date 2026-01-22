using Common.Constants;
using Order.Domain.Abstractions;

namespace Order.Domain.Entities;

public class OutBoxMessageEntity:Entity<Guid>
{

    #region Fields, Properties and Indexers

    public string? EventType { get; set; }
    public string? Content { get; set; }
    public DateTimeOffset OccurredOnUtc { get; set; }
    public DateTimeOffset? ProcessedOnUtc { get; set; }
    public string? LastErrorMessage { get; set; }
    public int AttemptCount { get; set; }
    public int MaxAttempts { get; set; }
    public DateTimeOffset? NextAttemptOnUtc { get; set; }

    #endregion

    #region Factories

    public static OutBoxMessageEntity Create(Guid id, string EventTypeFilter, string content, DateTimeOffset occurredOnUtc)
    {
        return new OutBoxMessageEntity()
        {
            Id = id,
            EventType = EventTypeFilter,
            Content = content,
            OccurredOnUtc = occurredOnUtc,
            AttemptCount = 0,
            MaxAttempts = AppConstants.MaxAttempts
        };
    }

    #endregion

    #region Methods

    public void CompleteProcessing(DateTimeOffset processedOnUtc, string? lastErrorMessage = null)
    {
        ProcessedOnUtc = processedOnUtc;
        LastErrorMessage = lastErrorMessage;
        NextAttemptOnUtc = null;
    }
    public void SetRetryProperties(int attemptCount, int maxAttempts, DateTimeOffset nextAttemptOnUtc, string? lastErrorMessage)
    {
        AttemptCount = attemptCount;
        MaxAttempts = maxAttempts;
        NextAttemptOnUtc = nextAttemptOnUtc;
        LastErrorMessage = lastErrorMessage;
    }
    public void RecordFailedAttempt(string errorMessage, DateTimeOffset currentTime)
    {
        InCreaseAttemptCount();
        if (AttemptCount >= MaxAttempts)
        {
            LastErrorMessage = $"Max attempts ({MaxAttempts}) exceeded. Last error: {errorMessage})";
            NextAttemptOnUtc = null;
        }
        else
        {
            // Calculate exponential backoff delay
            var baseDelay = TimeSpan.FromSeconds(Math.Pow(2, AttemptCount - 1));
            var maxDelay = TimeSpan.FromSeconds(5);
            var jitter = TimeSpan.FromMilliseconds(Random.Shared.Next(0, 1000));
            var delay = TimeSpan.FromTicks(Math.Min(baseDelay.Ticks, maxDelay.Ticks)) + jitter;
            NextAttemptOnUtc = currentTime + delay;
            LastErrorMessage = errorMessage;
        }
    }
    private void InCreaseAttemptCount()
    {
        AttemptCount++;
    }
    public bool CanRetry(DateTimeOffset currentTime)
    {
        return AttemptCount < MaxAttempts && (NextAttemptOnUtc is null || currentTime >= NextAttemptOnUtc.Value);
    }
    public bool IsPermanentlyFailed()
    {
        return AttemptCount >= MaxAttempts;
    }
    #endregion
}