using LogisticsPartnerHub.Domain.Entities;

namespace LogisticsPartnerHub.Domain.Interfaces.Services;

public interface IPartnerAuthenticator
{
    Task<HttpRequestMessage> AuthenticateAsync(HttpRequestMessage request, Partner partner, CancellationToken cancellationToken = default);
}
