using LogisticsPartnerHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LogisticsPartnerHub.Infrastructure.Data;

public class LogisticsPartnerDbContext : DbContext
{
    public LogisticsPartnerDbContext(DbContextOptions<LogisticsPartnerDbContext> options) : base(options) { }

    public DbSet<Partner> Partners => Set<Partner>();
    public DbSet<FieldMapping> FieldMappings => Set<FieldMapping>();
    public DbSet<ServiceOrder> ServiceOrders => Set<ServiceOrder>();
    public DbSet<ServiceOrderLog> ServiceOrderLogs => Set<ServiceOrderLog>();
    public DbSet<PartnerEndpoint> PartnerEndpoints => Set<PartnerEndpoint>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LogisticsPartnerDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
