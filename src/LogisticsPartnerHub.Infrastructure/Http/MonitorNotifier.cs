using System.Text;
using System.Text.Json;
using LogisticsPartnerHub.Domain.Entities;
using LogisticsPartnerHub.Domain.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LogisticsPartnerHub.Infrastructure.Http;

public class MonitorNotifier(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    ILogger<MonitorNotifier> logger) : IPartnerNotifier
{
    public async Task NotifyStatusChangeAsync(ServiceOrder serviceOrder, CancellationToken cancellationToken = default)
    {
        var baseUrl = configuration["Monitor:BaseUrl"];
        if (string.IsNullOrEmpty(baseUrl))
        {
            logger.LogWarning("Monitor:BaseUrl not configured, skipping status notification");
            return;
        }

        var payload = JsonSerializer.Serialize(new
        {
            serviceOrderId = serviceOrder.ExternalId,
            status = serviceOrder.Status.ToString(),
            partnerExternalId = serviceOrder.PartnerExternalId,
            updatedAt = serviceOrder.UpdatedAt
        });

        await SendNotificationAsync(baseUrl, "/api/notifications/status", payload, cancellationToken);
    }

    public async Task NotifyFailureAsync(ServiceOrder serviceOrder, string reason, CancellationToken cancellationToken = default)
    {
        var baseUrl = configuration["Monitor:BaseUrl"];
        if (string.IsNullOrEmpty(baseUrl))
        {
            logger.LogWarning("Monitor:BaseUrl not configured, skipping failure notification");
            return;
        }

        var payload = JsonSerializer.Serialize(new
        {
            serviceOrderId = serviceOrder.ExternalId,
            status = "Failed",
            reason,
            updatedAt = DateTime.UtcNow
        });

        await SendNotificationAsync(baseUrl, "/api/notifications/failure", payload, cancellationToken);
    }

    private async Task SendNotificationAsync(string baseUrl, string path, string payload, CancellationToken cancellationToken)
    {
        try
        {
            var httpClient = httpClientFactory.CreateClient("MonitorClient");
            var url = $"{baseUrl.TrimEnd('/')}{path}";

            var response = await httpClient.PostAsync(
                url,
                new StringContent(payload, Encoding.UTF8, "application/json"),
                cancellationToken);

            if (response.IsSuccessStatusCode)
                logger.LogInformation("Monitor notified successfully at {Url}", url);
            else
                logger.LogWarning("Monitor notification failed with status {StatusCode} at {Url}",
                    (int)response.StatusCode, url);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to notify Monitor");
        }
    }
}
