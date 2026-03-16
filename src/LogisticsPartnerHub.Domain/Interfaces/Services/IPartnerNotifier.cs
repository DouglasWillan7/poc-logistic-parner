using LogisticsPartnerHub.Domain.Entities;

namespace LogisticsPartnerHub.Domain.Interfaces.Services;

public interface IPartnerNotifier
{
    Task NotifyStatusChangeAsync(ServiceOrder serviceOrder, CancellationToken cancellationToken = default);
    Task NotifyFailureAsync(ServiceOrder serviceOrder, string reason, CancellationToken cancellationToken = default);
}
