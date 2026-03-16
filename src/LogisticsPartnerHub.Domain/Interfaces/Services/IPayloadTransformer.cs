using LogisticsPartnerHub.Domain.Enums;

namespace LogisticsPartnerHub.Domain.Interfaces.Services;

public interface IPayloadTransformer
{
    Task<string> TransformOutboundAsync(string canonicalPayload, Guid partnerId, ServiceType serviceType, CancellationToken cancellationToken = default);
    Task<string> TransformInboundAsync(string partnerPayload, Guid partnerId, ServiceType serviceType, CancellationToken cancellationToken = default);
}
