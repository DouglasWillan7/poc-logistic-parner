using LogisticsPartnerHub.Domain.Entities;
using LogisticsPartnerHub.Domain.Enums;
using LogisticsPartnerHub.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LogisticsPartnerHub.Infrastructure.Data.Repositories;

public class FieldMappingRepository(LogisticsPartnerDbContext context) : IFieldMappingRepository
{
    public async Task<IEnumerable<FieldMapping>> GetByPartnerAndServiceTypeAsync(
        Guid partnerId, ServiceType serviceType, MappingDirection direction,
        CancellationToken cancellationToken = default)
        => await context.FieldMappings
            .Where(f => f.PartnerId == partnerId && f.ServiceType == serviceType && f.Direction == direction)
            .OrderBy(f => f.Order)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<FieldMapping>> GetByPartnerIdAsync(Guid partnerId, CancellationToken cancellationToken = default)
        => await context.FieldMappings
            .Where(f => f.PartnerId == partnerId)
            .ToListAsync(cancellationToken);

    public async Task<FieldMapping?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.FieldMappings.FirstOrDefaultAsync(f => f.Id == id, cancellationToken);

    public async Task AddAsync(FieldMapping mapping, CancellationToken cancellationToken = default)
        => await context.FieldMappings.AddAsync(mapping, cancellationToken);

    public void Update(FieldMapping mapping)
        => context.FieldMappings.Update(mapping);

    public void Delete(FieldMapping mapping)
        => context.FieldMappings.Remove(mapping);
}
