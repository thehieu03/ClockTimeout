using System.Collections.Concurrent;
using Catalog.Domain.Repositories;
using Common.Configurations;
using MassTransit;

namespace Catalog.Worker.Outbox.Processors;

internal sealed class OutboxProcessor(
    IOutboxRepository outboxRepo,
    IConfiguration cfg,
    IPublishEndpoint publish,
    ILogger<OutboxProcessor> logger)
{
    private readonly int _batchSize = cfg.GetValue<int>($"{WorkerCfg.Outbox.Section}:{WorkerCfg.Outbox.BatchSize}",1000);
    private static readonly ConcurrentDictionary<string, Type> TypeCache = new();
    private readonly IOutboxRepository _outboxRepo = outboxRepo;
    private readonly IPublishEndpoint _publish = publish;
    private readonly ILogger<OutboxProcessor> _logger = logger;
    public async Task<int> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var newMessages = await _outboxRepo.GetAndClaimMessagesAsync(_batchSize, cancellationToken);
        // var retryMessages=await _outboxRepo
        return 0;
    }
}