using LogisticsPartnerHub.Domain.Enums;

namespace LogisticsPartnerHub.Domain.Entities;

public class FieldMapping
{
    public Guid Id { get; set; }
    public Guid PartnerId { get; set; }
    public MappingDirection Direction { get; set; }
    public string SourcePath { get; set; } = string.Empty;
    public string TargetPath { get; set; } = string.Empty;
    public string? DefaultValue { get; set; }
    public int Order { get; set; }
    public ServiceType ServiceType { get; set; }

    public Partner Partner { get; set; } = null!;
}
