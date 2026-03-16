using LogisticsPartnerHub.Application.Interfaces;
using LogisticsPartnerHub.Domain.Entities;
using LogisticsPartnerHub.Domain.Enums;
using LogisticsPartnerHub.Domain.Interfaces.Repositories;
using LogisticsPartnerHub.Domain.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LogisticsPartnerHub.Application.Commands.ServiceOrders;

public class ProcessServiceOrderHandler(
    IServiceOrderRepository serviceOrderRepository,
    IPartnerRepository partnerRepository,
    IPartnerEndpointRepository endpointRepository,
    IPayloadTransformer payloadTransformer,
    IPartnerHttpClient partnerHttpClient,
    IServiceOrderLogRepository logRepository,
    IUnitOfWork unitOfWork,
    ILogger<ProcessServiceOrderHandler> logger) : IRequestHandler<ProcessServiceOrderCommand, bool>
{
    public async Task<bool> Handle(ProcessServiceOrderCommand request, CancellationToken cancellationToken)
    {
        var serviceOrder = await serviceOrderRepository.GetByIdAsync(request.ServiceOrderId, cancellationToken)
            ?? throw new KeyNotFoundException($"ServiceOrder {request.ServiceOrderId} not found");

        var partner = await partnerRepository.GetByIdAsync(serviceOrder.PartnerId, cancellationToken)
            ?? throw new KeyNotFoundException($"Partner {serviceOrder.PartnerId} not found");

        var endpoint = await endpointRepository.GetByPartnerAndServiceTypeAsync(
            serviceOrder.PartnerId, serviceOrder.ServiceType, cancellationToken)
            ?? throw new InvalidOperationException("Endpoint not configured");

        // Aplica de-para nos campos
        var partnerPayload = await payloadTransformer.TransformOutboundAsync(
            serviceOrder.CanonicalPayload, serviceOrder.PartnerId,
            serviceOrder.ServiceType, cancellationToken);

        serviceOrder.PartnerPayload = partnerPayload;

        // Envia ao parceiro
        var url = $"{partner.BaseUrl.TrimEnd('/')}/{endpoint.Path.TrimStart('/')}";
        var (statusCode, responseBody) = await partnerHttpClient.SendAsync(
            partner, endpoint.HttpMethod, url, partnerPayload, cancellationToken);

        // Persiste log da interação
        var log = new ServiceOrderLog
        {
            Id = Guid.NewGuid(),
            ServiceOrderId = serviceOrder.Id,
            Direction = MappingDirection.Outbound,
            RequestPayload = partnerPayload,
            ResponsePayload = responseBody,
            HttpStatusCode = statusCode,
            AttemptNumber = serviceOrder.Logs.Count + 1,
            CreatedAt = DateTime.UtcNow
        };
        await logRepository.AddAsync(log, cancellationToken);

        if (statusCode >= 200 && statusCode < 300)
        {
            serviceOrder.Status = ServiceOrderStatus.Aceito;
            logger.LogInformation("ServiceOrder {Id} accepted by partner {Partner}", serviceOrder.Id, partner.Name);
        }
        else
        {
            logger.LogWarning("ServiceOrder {Id} rejected by partner {Partner} with status {StatusCode}",
                serviceOrder.Id, partner.Name, statusCode);
        }

        serviceOrder.UpdatedAt = DateTime.UtcNow;
        serviceOrderRepository.Update(serviceOrder);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return statusCode >= 200 && statusCode < 300;
    }
}
