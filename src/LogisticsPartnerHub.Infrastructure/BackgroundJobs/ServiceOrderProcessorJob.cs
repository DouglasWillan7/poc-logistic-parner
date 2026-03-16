using LogisticsPartnerHub.Application.Commands.ServiceOrders;
using LogisticsPartnerHub.Domain.Enums;
using LogisticsPartnerHub.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LogisticsPartnerHub.Infrastructure.BackgroundJobs;

public class ServiceOrderProcessorJob(
    IServiceProvider serviceProvider,
    ILogger<ServiceOrderProcessorJob> logger) : BackgroundService
{
    private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("ServiceOrderProcessorJob started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingOrdersAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Error processing pending service orders");
            }

            await Task.Delay(PollingInterval, stoppingToken);
        }
    }

    private async Task ProcessPendingOrdersAsync(CancellationToken stoppingToken)
    {
        using var scope = serviceProvider.CreateScope();
        var serviceOrderRepository = scope.ServiceProvider.GetRequiredService<IServiceOrderRepository>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var pendingOrders = await serviceOrderRepository.GetByStatusAsync(
            ServiceOrderStatus.Solicitado, stoppingToken);

        foreach (var order in pendingOrders)
        {
            if (stoppingToken.IsCancellationRequested) break;

            try
            {
                logger.LogInformation("Processing ServiceOrder {Id}", order.Id);
                var success = await mediator.Send(new ProcessServiceOrderCommand(order.Id), stoppingToken);

                if (!success)
                {
                    logger.LogWarning("ServiceOrder {Id} processing returned failure, enqueuing for retry", order.Id);
                    await EnqueueForRetryAsync(scope.ServiceProvider, order.Id, stoppingToken);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Failed to process ServiceOrder {Id}, enqueuing for retry", order.Id);
                await EnqueueForRetryAsync(scope.ServiceProvider, order.Id, stoppingToken);
            }
        }
    }

    private static async Task EnqueueForRetryAsync(
        IServiceProvider serviceProvider, Guid serviceOrderId, CancellationToken cancellationToken)
    {
        var retryQueue = serviceProvider.GetRequiredService<IRetryQueue>();
        await retryQueue.EnqueueAsync(serviceOrderId, cancellationToken);
    }
}
