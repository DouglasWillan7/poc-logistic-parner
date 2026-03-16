using LogisticsPartnerHub.Domain.Enums;

namespace LogisticsPartnerHub.Domain.Entities;

public class PartnerEndpoint
{
    public Guid Id { get; set; }
    public Guid PartnerId { get; set; }
    public ServiceType ServiceType { get; set; }
    public string HttpMethod { get; set; } = "POST";
    public string Path { get; set; } = string.Empty;

    public Partner Partner { get; set; } = null!;
}
