using LogisticsPartnerHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LogisticsPartnerHub.Infrastructure.Data.Configurations;

public class FieldMappingConfiguration : IEntityTypeConfiguration<FieldMapping>
{
    public void Configure(EntityTypeBuilder<FieldMapping> builder)
    {
        builder.ToTable("field_mappings");

        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id).HasColumnName("id");
        builder.Property(f => f.PartnerId).HasColumnName("partner_id").IsRequired();
        builder.Property(f => f.Direction).HasColumnName("direction").IsRequired();
        builder.Property(f => f.SourcePath).HasColumnName("source_path").HasMaxLength(500).IsRequired();
        builder.Property(f => f.TargetPath).HasColumnName("target_path").HasMaxLength(500).IsRequired();
        builder.Property(f => f.DefaultValue).HasColumnName("default_value").HasMaxLength(1000);
        builder.Property(f => f.Order).HasColumnName("order").HasDefaultValue(0);
        builder.Property(f => f.ServiceType).HasColumnName("service_type").IsRequired();

        builder.HasIndex(f => new { f.PartnerId, f.ServiceType, f.Direction });
    }
}
