using LogisticsPartnerHub.Domain.Entities;
using LogisticsPartnerHub.Domain.Enums;
using LogisticsPartnerHub.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LogisticsPartnerHub.Infrastructure.Data.Repositories;

public class ServiceOrderRepository(LogisticsPartnerDbContext context) : IServiceOrderRepository
{
    public async Task<ServiceOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.ServiceOrders
            .Include(s => s.Partner)
            .Include(s => s.Logs)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public async Task<ServiceOrder?> GetByExternalIdAsync(string externalId, CancellationToken cancellationToken = default)
        => await context.ServiceOrders
            .Include(s => s.Partner)
            .FirstOrDefaultAsync(s => s.ExternalId == externalId, cancellationToken);

    public async Task<IEnumerable<ServiceOrder>> GetByStatusAsync(ServiceOrderStatus status, CancellationToken cancellationToken = default)
        => await context.ServiceOrders
            .Where(s => s.Status == status)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<ServiceOrder>> GetAllAsync(CancellationToken cancellationToken = default)
        => await context.ServiceOrders
            .Include(s => s.Partner)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(ServiceOrder serviceOrder, CancellationToken cancellationToken = default)
        => await context.ServiceOrders.AddAsync(serviceOrder, cancellationToken);

    public void Update(ServiceOrder serviceOrder)
        => context.ServiceOrders.Update(serviceOrder);
}
