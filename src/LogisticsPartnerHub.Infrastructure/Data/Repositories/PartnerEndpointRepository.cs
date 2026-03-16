using LogisticsPartnerHub.Domain.Entities;
using LogisticsPartnerHub.Domain.Enums;
using LogisticsPartnerHub.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LogisticsPartnerHub.Infrastructure.Data.Repositories;

public class PartnerEndpointRepository(LogisticsPartnerDbContext context) : IPartnerEndpointRepository
{
    public async Task<PartnerEndpoint?> GetByPartnerAndServiceTypeAsync(Guid partnerId, ServiceType serviceType, CancellationToken cancellationToken = default)
        => await context.PartnerEndpoints
            .FirstOrDefaultAsync(e => e.PartnerId == partnerId && e.ServiceType == serviceType, cancellationToken);

    public async Task<IEnumerable<PartnerEndpoint>> GetByPartnerIdAsync(Guid partnerId, CancellationToken cancellationToken = default)
        => await context.PartnerEndpoints
            .Where(e => e.PartnerId == partnerId)
            .ToListAsync(cancellationToken);

    public async Task<PartnerEndpoint?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.PartnerEndpoints.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public async Task AddAsync(PartnerEndpoint endpoint, CancellationToken cancellationToken = default)
        => await context.PartnerEndpoints.AddAsync(endpoint, cancellationToken);

    public void Update(PartnerEndpoint endpoint)
        => context.PartnerEndpoints.Update(endpoint);

    public void Delete(PartnerEndpoint endpoint)
        => context.PartnerEndpoints.Remove(endpoint);
}
