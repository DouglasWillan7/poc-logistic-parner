using System.Text;
using LogisticsPartnerHub.Application.Interfaces;
using LogisticsPartnerHub.Domain.Entities;
using LogisticsPartnerHub.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace LogisticsPartnerHub.Infrastructure.Http;

public class PartnerHttpClient(
    IHttpClientFactory httpClientFactory,
    IPartnerAuthenticator authenticator,
    ILogger<PartnerHttpClient> logger) : IPartnerHttpClient
{
    public async Task<(int StatusCode, string ResponseBody)> SendAsync(
        Partner partner, string httpMethod, string url, string payload,
        CancellationToken cancellationToken = default)
    {
        var httpClient = httpClientFactory.CreateClient("PartnerClient");

        var request = new HttpRequestMessage(new HttpMethod(httpMethod), url)
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        };

        request = await authenticator.AuthenticateAsync(request, partner, cancellationToken);

        logger.LogInformation("Sending {Method} request to partner {Partner} at {Url}",
            httpMethod, partner.Name, url);

        var response = await httpClient.SendAsync(request, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        logger.LogInformation("Partner {Partner} responded with status {StatusCode}",
            partner.Name, (int)response.StatusCode);

        return ((int)response.StatusCode, responseBody);
    }
}
