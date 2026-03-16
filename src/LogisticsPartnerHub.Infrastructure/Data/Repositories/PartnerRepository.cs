using LogisticsPartnerHub.Domain.Entities;
using LogisticsPartnerHub.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LogisticsPartnerHub.Infrastructure.Data.Repositories;

public class PartnerRepository(LogisticsPartnerDbContext context) : IPartnerRepository
{
    public async Task<Partner?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.Partners.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<IEnumerable<Partner>> GetAllAsync(CancellationToken cancellationToken = default)
        => await context.Partners.ToListAsync(cancellationToken);

    public async Task AddAsync(Partner partner, CancellationToken cancellationToken = default)
        => await context.Partners.AddAsync(partner, cancellationToken);

    public void Update(Partner partner)
        => context.Partners.Update(partner);
}
