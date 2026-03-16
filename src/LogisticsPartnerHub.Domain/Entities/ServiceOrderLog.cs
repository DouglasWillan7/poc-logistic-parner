using LogisticsPartnerHub.Domain.Enums;

namespace LogisticsPartnerHub.Domain.Entities;

public class ServiceOrderLog
{
    public Guid Id { get; set; }
    public Guid ServiceOrderId { get; set; }
    public MappingDirection Direction { get; set; }
    public string RequestPayload { get; set; } = string.Empty;
    public string? ResponsePayload { get; set; }
    public int? HttpStatusCode { get; set; }
    public int AttemptNumber { get; set; }
    public DateTime CreatedAt { get; set; }

    public ServiceOrder ServiceOrder { get; set; } = null!;
}
