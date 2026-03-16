namespace LogisticsPartnerHub.Infrastructure.BackgroundJobs;

public interface IRetryQueue
{
    Task EnqueueAsync(Guid serviceOrderId, CancellationToken cancellationToken = default);
    Task<RetryItem?> DequeueAsync(CancellationToken cancellationToken = default);
    Task<int> GetCountAsync(CancellationToken cancellationToken = default);
}

public record RetryItem(Guid ServiceOrderId, DateTime EnqueuedAt, int RetryCount);
