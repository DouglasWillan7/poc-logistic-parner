using LogisticsPartnerHub.Domain.Entities;
using LogisticsPartnerHub.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LogisticsPartnerHub.Infrastructure.Data.Repositories;

public class ServiceOrderLogRepository(LogisticsPartnerDbContext context) : IServiceOrderLogRepository
{
    public async Task AddAsync(ServiceOrderLog log, CancellationToken cancellationToken = default)
        => await context.ServiceOrderLogs.AddAsync(log, cancellationToken);

    public async Task<IEnumerable<ServiceOrderLog>> GetByServiceOrderIdAsync(Guid serviceOrderId, CancellationToken cancellationToken = default)
        => await context.ServiceOrderLogs
            .Where(l => l.ServiceOrderId == serviceOrderId)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync(cancellationToken);
}
