using Payment.Domain.Abstractions;

namespace Payment.Domain.Entities;

public class PaymentWebhookLog: Entity<Guid>
{
    public string Gateway { get; private set; } = default!; // "Momo", "VnPay"
    public string RequestId { get; private set; } = default!; // Unique ID from Gateway
    public string Content { get; private set; } = default!; // JSON payload
    public bool IsProcessed { get; private set; }
    public string? ErrorMessage { get; private set; }

    public static PaymentWebhookLog Create(string gateway, string requestId, string content)
    {
        return new PaymentWebhookLog
        {
            Id = Guid.NewGuid(),
            Gateway = gateway,
            RequestId = requestId,
            Content = content,
            IsProcessed = false,
            CreatedOnUtc = DateTimeOffset.UtcNow
        };
    }

    public void MarkProcessed()
    {
        IsProcessed = true;
        LastModifiedOnUtc = DateTimeOffset.UtcNow;
    }

    public void MarkFailed(string error)
    {
        IsProcessed = false;
        ErrorMessage = error;
        LastModifiedOnUtc = DateTimeOffset.UtcNow;
    }
}