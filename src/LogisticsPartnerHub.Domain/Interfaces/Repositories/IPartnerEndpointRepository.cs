using LogisticsPartnerHub.Domain.Entities;
using LogisticsPartnerHub.Domain.Enums;

namespace LogisticsPartnerHub.Domain.Interfaces.Repositories;

public interface IPartnerEndpointRepository
{
    Task<PartnerEndpoint?> GetByPartnerAndServiceTypeAsync(Guid partnerId, ServiceType serviceType, CancellationToken cancellationToken = default);
    Task<IEnumerable<PartnerEndpoint>> GetByPartnerIdAsync(Guid partnerId, CancellationToken cancellationToken = default);
    Task<PartnerEndpoint?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(PartnerEndpoint endpoint, CancellationToken cancellationToken = default);
    void Update(PartnerEndpoint endpoint);
    void Delete(PartnerEndpoint endpoint);
}
