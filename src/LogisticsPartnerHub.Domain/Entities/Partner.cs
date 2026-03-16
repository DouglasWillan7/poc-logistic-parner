using LogisticsPartnerHub.Domain.Enums;

namespace LogisticsPartnerHub.Domain.Entities;

public class Partner
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public AuthType AuthType { get; set; }
    public string AuthConfig { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<FieldMapping> FieldMappings { get; set; } = [];
    public ICollection<PartnerEndpoint> Endpoints { get; set; } = [];
    public ICollection<ServiceOrder> ServiceOrders { get; set; } = [];
}
