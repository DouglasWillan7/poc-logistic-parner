using LogisticsPartnerHub.Domain.Entities;
using LogisticsPartnerHub.Domain.Enums;
using LogisticsPartnerHub.Domain.Interfaces.Services;

namespace LogisticsPartnerHub.Infrastructure.Http.Authenticators;

public class PartnerAuthenticatorFactory(
    ApiKeyAuthenticator apiKeyAuthenticator,
    BasicAuthAuthenticator basicAuthAuthenticator,
    OAuth2Authenticator oAuth2Authenticator) : IPartnerAuthenticator
{
    public Task<HttpRequestMessage> AuthenticateAsync(HttpRequestMessage request, Partner partner, CancellationToken cancellationToken = default)
    {
        return partner.AuthType switch
        {
            AuthType.ApiKey => apiKeyAuthenticator.AuthenticateAsync(request, partner, cancellationToken),
            AuthType.BasicAuth => basicAuthAuthenticator.AuthenticateAsync(request, partner, cancellationToken),
            AuthType.OAuth2 => oAuth2Authenticator.AuthenticateAsync(request, partner, cancellationToken),
            _ => throw new NotSupportedException($"Auth type {partner.AuthType} is not supported")
        };
    }
}
