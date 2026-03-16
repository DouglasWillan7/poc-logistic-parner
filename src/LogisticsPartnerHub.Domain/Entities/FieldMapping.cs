using LogisticsPartnerHub.Domain.Enums;

namespace LogisticsPartnerHub.Domain.Entities;

public class FieldMapping
{
    public Guid Id { get; set; }
    public Guid PartnerId { get; set; }
    public MappingDirection Direction { get; set; }
    public string SourceField { get; set; } = string.Empty;
    public string TargetField { get; set; } = string.Empty;
    public ServiceType ServiceType { get; set; }

    public Partner Partner { get; set; } = null!;
}
