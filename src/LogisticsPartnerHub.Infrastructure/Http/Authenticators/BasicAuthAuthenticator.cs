using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using LogisticsPartnerHub.Domain.Entities;

namespace LogisticsPartnerHub.Infrastructure.Http.Authenticators;

public class BasicAuthAuthenticator
{
    public Task<HttpRequestMessage> AuthenticateAsync(HttpRequestMessage request, Partner partner, CancellationToken cancellationToken = default)
    {
        var config = JsonDocument.Parse(partner.AuthConfig).RootElement;

        var username = config.GetProperty("username").GetString()!;
        var password = config.GetProperty("password").GetString()!;

        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);

        return Task.FromResult(request);
    }
}
