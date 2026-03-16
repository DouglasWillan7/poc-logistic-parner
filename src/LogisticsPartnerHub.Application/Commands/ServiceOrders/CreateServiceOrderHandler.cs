using LogisticsPartnerHub.Application.DTOs;
using LogisticsPartnerHub.Domain.Entities;
using LogisticsPartnerHub.Domain.Enums;
using LogisticsPartnerHub.Domain.Interfaces.Repositories;
using MediatR;

namespace LogisticsPartnerHub.Application.Commands.ServiceOrders;

public class CreateServiceOrderHandler(
    IServiceOrderRepository serviceOrderRepository,
    IPartnerRepository partnerRepository,
    IFieldMappingRepository fieldMappingRepository,
    IPartnerEndpointRepository endpointRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateServiceOrderCommand, ServiceOrderDto>
{
    public async Task<ServiceOrderDto> Handle(CreateServiceOrderCommand request, CancellationToken cancellationToken)
    {
        // Idempotência: se já existe, retorna a existente
        var existing = await serviceOrderRepository.GetByExternalIdAsync(request.ExternalId, cancellationToken);
        if (existing is not null)
        {
            return new ServiceOrderDto(
                existing.Id, existing.ExternalId, existing.PartnerId,
                existing.Partner?.Name, existing.ServiceType, existing.Status,
                existing.PartnerExternalId, existing.CreatedAt, existing.UpdatedAt);
        }

        var partner = await partnerRepository.GetByIdAsync(request.PartnerId, cancellationToken)
            ?? throw new KeyNotFoundException($"Partner {request.PartnerId} not found");

        if (!partner.IsActive)
            throw new InvalidOperationException($"Partner {partner.Name} is not active");

        // Valida que existem mapeamentos configurados
        var mappings = await fieldMappingRepository.GetByPartnerAndServiceTypeAsync(
            request.PartnerId, request.ServiceType, MappingDirection.Outbound, cancellationToken);
        if (!mappings.Any())
            throw new InvalidOperationException(
                $"No outbound field mappings configured for partner {partner.Name} and service type {request.ServiceType}");

        // Valida que existe endpoint configurado
        var endpoint = await endpointRepository.GetByPartnerAndServiceTypeAsync(
            request.PartnerId, request.ServiceType, cancellationToken)
            ?? throw new InvalidOperationException(
                $"No endpoint configured for partner {partner.Name} and service type {request.ServiceType}");

        var serviceOrder = new ServiceOrder
        {
            Id = Guid.NewGuid(),
            ExternalId = request.ExternalId,
            PartnerId = request.PartnerId,
            ServiceType = request.ServiceType,
            Status = ServiceOrderStatus.Solicitado,
            CanonicalPayload = request.Payload,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await serviceOrderRepository.AddAsync(serviceOrder, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new ServiceOrderDto(
            serviceOrder.Id, serviceOrder.ExternalId, serviceOrder.PartnerId,
            partner.Name, serviceOrder.ServiceType, serviceOrder.Status,
            serviceOrder.PartnerExternalId, serviceOrder.CreatedAt, serviceOrder.UpdatedAt);
    }
}
