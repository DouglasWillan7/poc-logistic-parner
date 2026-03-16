using LogisticsPartnerHub.Domain.Entities;
using LogisticsPartnerHub.Domain.Enums;

namespace LogisticsPartnerHub.Domain.Interfaces.Repositories;

public interface IFieldMappingRepository
{
    Task<IEnumerable<FieldMapping>> GetByPartnerAndServiceTypeAsync(
        Guid partnerId, ServiceType serviceType, MappingDirection direction,
        CancellationToken cancellationToken = default);
    Task<IEnumerable<FieldMapping>> GetByPartnerIdAsync(Guid partnerId, CancellationToken cancellationToken = default);
    Task<FieldMapping?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(FieldMapping mapping, CancellationToken cancellationToken = default);
    void Update(FieldMapping mapping);
    void Delete(FieldMapping mapping);
}
