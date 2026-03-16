using LogisticsPartnerHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LogisticsPartnerHub.Infrastructure.Data.Configurations;

public class PartnerConfiguration : IEntityTypeConfiguration<Partner>
{
    public void Configure(EntityTypeBuilder<Partner> builder)
    {
        builder.ToTable("partners");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id");
        builder.Property(p => p.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(p => p.BaseUrl).HasColumnName("base_url").HasMaxLength(500).IsRequired();
        builder.Property(p => p.AuthType).HasColumnName("auth_type").IsRequired();
        builder.Property(p => p.AuthConfig).HasColumnName("auth_config").HasColumnType("jsonb").IsRequired();
        builder.Property(p => p.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        builder.Property(p => p.CreatedAt).HasColumnName("created_at");
        builder.Property(p => p.UpdatedAt).HasColumnName("updated_at");

        builder.HasMany(p => p.FieldMappings).WithOne(f => f.Partner).HasForeignKey(f => f.PartnerId);
        builder.HasMany(p => p.Endpoints).WithOne(e => e.Partner).HasForeignKey(e => e.PartnerId);
        builder.HasMany(p => p.ServiceOrders).WithOne(s => s.Partner).HasForeignKey(s => s.PartnerId);
    }
}
