using LogisticsPartnerHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LogisticsPartnerHub.Infrastructure.Data.Configurations;

public class ServiceOrderLogConfiguration : IEntityTypeConfiguration<ServiceOrderLog>
{
    public void Configure(EntityTypeBuilder<ServiceOrderLog> builder)
    {
        builder.ToTable("service_order_logs");

        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).HasColumnName("id");
        builder.Property(l => l.ServiceOrderId).HasColumnName("service_order_id").IsRequired();
        builder.Property(l => l.Direction).HasColumnName("direction").IsRequired();
        builder.Property(l => l.RequestPayload).HasColumnName("request_payload").HasColumnType("jsonb").IsRequired();
        builder.Property(l => l.ResponsePayload).HasColumnName("response_payload").HasColumnType("jsonb");
        builder.Property(l => l.HttpStatusCode).HasColumnName("http_status_code");
        builder.Property(l => l.AttemptNumber).HasColumnName("attempt_number");
        builder.Property(l => l.CreatedAt).HasColumnName("created_at");

        builder.HasIndex(l => l.ServiceOrderId);
    }
}
