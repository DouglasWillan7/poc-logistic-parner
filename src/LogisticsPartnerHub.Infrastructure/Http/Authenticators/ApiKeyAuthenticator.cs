using System.Text.Json;
using LogisticsPartnerHub.Domain.Entities;
using LogisticsPartnerHub.Domain.Enums;

namespace LogisticsPartnerHub.Infrastructure.Http.Authenticators;

public class ApiKeyAuthenticator
{
    public Task<HttpRequestMessage> AuthenticateAsync(HttpRequestMessage request, Partner partner, CancellationToken cancellationToken = default)
    {
        var config = JsonDocument.Parse(partner.AuthConfig).RootElement;

        var headerName = config.TryGetProperty("headerName", out var h) ? h.GetString()! : "X-Api-Key";
        var apiKey = config.GetProperty("apiKey").GetString()!;

        request.Headers.TryAddWithoutValidation(headerName, apiKey);

        return Task.FromResult(request);
    }
}
