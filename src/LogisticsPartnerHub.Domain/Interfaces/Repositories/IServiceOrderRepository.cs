using LogisticsPartnerHub.Domain.Entities;
using LogisticsPartnerHub.Domain.Enums;

namespace LogisticsPartnerHub.Domain.Interfaces.Repositories;

public interface IServiceOrderRepository
{
    Task<ServiceOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ServiceOrder?> GetByExternalIdAsync(string externalId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ServiceOrder>> GetByStatusAsync(ServiceOrderStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<ServiceOrder>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(ServiceOrder serviceOrder, CancellationToken cancellationToken = default);
    void Update(ServiceOrder serviceOrder);
}
