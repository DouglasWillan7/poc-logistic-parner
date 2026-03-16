using LogisticsPartnerHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LogisticsPartnerHub.Infrastructure.Data.Configurations;

public class ServiceOrderConfiguration : IEntityTypeConfiguration<ServiceOrder>
{
    public void Configure(EntityTypeBuilder<ServiceOrder> builder)
    {
        builder.ToTable("service_orders");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnName("id");
        builder.Property(s => s.ExternalId).HasColumnName("external_id").HasMaxLength(200).IsRequired();
        builder.Property(s => s.PartnerId).HasColumnName("partner_id").IsRequired();
        builder.Property(s => s.ServiceType).HasColumnName("service_type").IsRequired();
        builder.Property(s => s.Status).HasColumnName("status").IsRequired();
        builder.Property(s => s.CanonicalPayload).HasColumnName("canonical_payload").HasColumnType("jsonb").IsRequired();
        builder.Property(s => s.PartnerPayload).HasColumnName("partner_payload").HasColumnType("jsonb");
        builder.Property(s => s.PartnerExternalId).HasColumnName("partner_external_id").HasMaxLength(200);
        builder.Property(s => s.CreatedAt).HasColumnName("created_at");
        builder.Property(s => s.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(s => s.ExternalId).IsUnique();
        builder.HasIndex(s => s.Status);

        builder.HasMany(s => s.Logs).WithOne(l => l.ServiceOrder).HasForeignKey(l => l.ServiceOrderId);
    }
}
