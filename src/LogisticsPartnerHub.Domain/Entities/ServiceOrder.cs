using LogisticsPartnerHub.Domain.Enums;

namespace LogisticsPartnerHub.Domain.Entities;

public class ServiceOrder
{
    public Guid Id { get; set; }
    public string ExternalId { get; set; } = string.Empty;
    public Guid PartnerId { get; set; }
    public ServiceType ServiceType { get; set; }
    public ServiceOrderStatus Status { get; set; }
    public string CanonicalPayload { get; set; } = string.Empty;
    public string? PartnerPayload { get; set; }
    public string? PartnerExternalId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Partner Partner { get; set; } = null!;
    public ICollection<ServiceOrderLog> Logs { get; set; } = [];
}
