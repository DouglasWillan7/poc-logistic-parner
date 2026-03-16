using LogisticsPartnerHub.Domain.Interfaces.Repositories;

namespace LogisticsPartnerHub.Infrastructure.Data;

public class UnitOfWork(LogisticsPartnerDbContext context) : IUnitOfWork
{
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await context.SaveChangesAsync(cancellationToken);
}
