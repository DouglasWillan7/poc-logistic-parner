using System.Collections.Concurrent;

namespace LogisticsPartnerHub.Infrastructure.BackgroundJobs;

public class InMemoryRetryQueue : IRetryQueue
{
    private readonly ConcurrentQueue<RetryItem> _queue = new();

    public Task EnqueueAsync(Guid serviceOrderId, CancellationToken cancellationToken = default)
    {
        _queue.Enqueue(new RetryItem(serviceOrderId, DateTime.UtcNow, 0));
        return Task.CompletedTask;
    }

    public Task<RetryItem?> DequeueAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_queue.TryDequeue(out var item) ? item : null);
    }

    public Task<int> GetCountAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_queue.Count);
    }
}
