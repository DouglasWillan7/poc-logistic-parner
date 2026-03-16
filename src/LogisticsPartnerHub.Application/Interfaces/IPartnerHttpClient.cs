using LogisticsPartnerHub.Domain.Entities;

namespace LogisticsPartnerHub.Application.Interfaces;

public interface IPartnerHttpClient
{
    Task<(int StatusCode, string ResponseBody)> SendAsync(
        Partner partner, string httpMethod, string url, string payload,
        CancellationToken cancellationToken = default);
}
