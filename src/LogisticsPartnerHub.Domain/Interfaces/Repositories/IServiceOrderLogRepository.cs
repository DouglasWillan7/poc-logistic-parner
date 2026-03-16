using LogisticsPartnerHub.Domain.Entities;

namespace LogisticsPartnerHub.Domain.Interfaces.Repositories;

public interface IServiceOrderLogRepository
{
    Task AddAsync(ServiceOrderLog log, CancellationToken cancellationToken = default);
    Task<IEnumerable<ServiceOrderLog>> GetByServiceOrderIdAsync(Guid serviceOrderId, CancellationToken cancellationToken = default);
}
