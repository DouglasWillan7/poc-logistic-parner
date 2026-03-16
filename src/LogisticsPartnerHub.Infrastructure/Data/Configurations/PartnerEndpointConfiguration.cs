using LogisticsPartnerHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LogisticsPartnerHub.Infrastructure.Data.Configurations;

public class PartnerEndpointConfiguration : IEntityTypeConfiguration<PartnerEndpoint>
{
    public void Configure(EntityTypeBuilder<PartnerEndpoint> builder)
    {
        builder.ToTable("partner_endpoints");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.PartnerId).HasColumnName("partner_id").IsRequired();
        builder.Property(e => e.ServiceType).HasColumnName("service_type").IsRequired();
        builder.Property(e => e.HttpMethod).HasColumnName("http_method").HasMaxLength(10).IsRequired();
        builder.Property(e => e.Path).HasColumnName("path").HasMaxLength(500).IsRequired();

        builder.HasIndex(e => new { e.PartnerId, e.ServiceType }).IsUnique();
    }
}
