using LogisticsPartnerHub.Domain.Entities;

namespace LogisticsPartnerHub.Domain.Interfaces.Repositories;

public interface IPartnerRepository
{
    Task<Partner?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Partner>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Partner partner, CancellationToken cancellationToken = default);
    void Update(Partner partner);
}
