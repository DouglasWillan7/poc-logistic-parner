using System.Net.Http.Headers;
using System.Text.Json;
using LogisticsPartnerHub.Domain.Entities;

namespace LogisticsPartnerHub.Infrastructure.Http.Authenticators;

public class OAuth2Authenticator(IHttpClientFactory httpClientFactory)
{
    public async Task<HttpRequestMessage> AuthenticateAsync(HttpRequestMessage request, Partner partner, CancellationToken cancellationToken = default)
    {
        var config = JsonDocument.Parse(partner.AuthConfig).RootElement;

        var tokenUrl = config.GetProperty("tokenUrl").GetString()!;
        var clientId = config.GetProperty("clientId").GetString()!;
        var clientSecret = config.GetProperty("clientSecret").GetString()!;

        var scope = config.TryGetProperty("scope", out var s) ? s.GetString() : null;

        var tokenRequest = new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = clientId,
            ["client_secret"] = clientSecret
        };

        if (!string.IsNullOrEmpty(scope))
            tokenRequest["scope"] = scope;

        var httpClient = httpClientFactory.CreateClient("OAuth2Token");
        var tokenResponse = await httpClient.PostAsync(
            tokenUrl,
            new FormUrlEncodedContent(tokenRequest),
            cancellationToken);

        tokenResponse.EnsureSuccessStatusCode();

        var responseBody = await tokenResponse.Content.ReadAsStringAsync(cancellationToken);
        var tokenJson = JsonDocument.Parse(responseBody).RootElement;
        var accessToken = tokenJson.GetProperty("access_token").GetString()!;

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        return request;
    }
}
