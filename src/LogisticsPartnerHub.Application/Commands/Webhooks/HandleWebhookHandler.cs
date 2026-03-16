using System.Text.Json;
using LogisticsPartnerHub.Domain.Entities;
using LogisticsPartnerHub.Domain.Enums;
using LogisticsPartnerHub.Domain.Interfaces.Repositories;
using LogisticsPartnerHub.Domain.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LogisticsPartnerHub.Application.Commands.Webhooks;

public class HandleWebhookHandler(
    IPartnerRepository partnerRepository,
    IServiceOrderRepository serviceOrderRepository,
    IServiceOrderLogRepository logRepository,
    IPayloadTransformer payloadTransformer,
    IPartnerNotifier partnerNotifier,
    IUnitOfWork unitOfWork,
    ILogger<HandleWebhookHandler> logger) : IRequestHandler<HandleWebhookCommand, bool>
{
    public async Task<bool> Handle(HandleWebhookCommand request, CancellationToken cancellationToken)
    {
        var partner = await partnerRepository.GetByIdAsync(request.PartnerId, cancellationToken);
        if (partner is null)
        {
            logger.LogWarning("Webhook received for unknown partner {PartnerId}", request.PartnerId);
            return false;
        }

        // Tenta extrair o external_id do payload para encontrar a service order
        // O de-para reverso transforma o payload do parceiro para o formato canônico
        // Precisamos identificar a service order pelo partner_external_id
        var jsonDoc = JsonDocument.Parse(request.Payload);
        var root = jsonDoc.RootElement;

        // Busca a service order - tenta por partner_external_id se disponível no payload
        string? partnerExternalId = null;
        if (root.TryGetProperty("order_id", out var orderIdProp))
            partnerExternalId = orderIdProp.GetString();
        else if (root.TryGetProperty("external_id", out var externalIdProp))
            partnerExternalId = externalIdProp.GetString();
        else if (root.TryGetProperty("id", out var idProp))
            partnerExternalId = idProp.GetString();

        if (partnerExternalId is null)
        {
            logger.LogWarning("Webhook payload from partner {Partner} missing order identifier", partner.Name);
            return false;
        }

        // Busca por external_id (que pode ser o partner_external_id)
        var serviceOrder = await serviceOrderRepository.GetByExternalIdAsync(partnerExternalId, cancellationToken);
        if (serviceOrder is null)
        {
            logger.LogWarning("No service order found for partner external id {ExternalId}", partnerExternalId);
            return false;
        }

        // Persiste log da interação inbound
        var log = new ServiceOrderLog
        {
            Id = Guid.NewGuid(),
            ServiceOrderId = serviceOrder.Id,
            Direction = MappingDirection.Inbound,
            RequestPayload = request.Payload,
            AttemptNumber = 1,
            CreatedAt = DateTime.UtcNow
        };
        await logRepository.AddAsync(log, cancellationToken);

        // Aplica de-para reverso para normalizar status
        var normalizedPayload = await payloadTransformer.TransformInboundAsync(
            request.Payload, request.PartnerId, serviceOrder.ServiceType, cancellationToken);

        var normalizedJson = JsonDocument.Parse(normalizedPayload);
        if (normalizedJson.RootElement.TryGetProperty("status", out var statusProp))
        {
            var statusStr = statusProp.GetString();
            if (Enum.TryParse<ServiceOrderStatus>(statusStr, true, out var newStatus))
            {
                serviceOrder.Status = newStatus;
                serviceOrder.UpdatedAt = DateTime.UtcNow;
                serviceOrderRepository.Update(serviceOrder);

                logger.LogInformation("ServiceOrder {Id} status updated to {Status} via webhook",
                    serviceOrder.Id, newStatus);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Notifica Monitor sobre mudança de status
        await partnerNotifier.NotifyStatusChangeAsync(serviceOrder, cancellationToken);

        return true;
    }
}
