using Catalog.Domain.Entities;

namespace Catalog.Domain.Repositories;

public interface IOutboxRepository
{
    #region Methods

    Task<bool> AddMessageAsync(OutboxMessageEntity message, CancellationToken cancellationToken = default);

    Task<bool> UpdateMessagesAsync(IEnumerable<OutboxMessageEntity> messages, CancellationToken cancellationToken = default);

    Task<List<OutboxMessageEntity>> GetAndClaimMessagesAsync(int batchSize, CancellationToken cancellationToken = default);

    Task<List<OutboxMessageEntity>> GetAndClaimRetryMessagesAsync(int batchSize, CancellationToken cancellationToken = default);

    Task<bool> ReleaseClaimsAsync(IEnumerable<OutboxMessageEntity> messages, CancellationToken cancellationToken = default);

    Task<bool> ReleaseExpiredClaimsAsync(TimeSpan claimTimeout, CancellationToken cancellationToken = default);

    #endregion
}