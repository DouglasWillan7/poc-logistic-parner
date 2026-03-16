using LogisticsPartnerHub.Application.DTOs;
using LogisticsPartnerHub.Domain.Interfaces.Repositories;
using MediatR;

namespace LogisticsPartnerHub.Application.Queries;

public class GetPartnerHandler(IPartnerRepository partnerRepository)
    : IRequestHandler<GetPartnerQuery, PartnerDto?>
{
    public async Task<PartnerDto?> Handle(GetPartnerQuery request, CancellationToken cancellationToken)
    {
        var partner = await partnerRepository.GetByIdAsync(request.Id, cancellationToken);
        if (partner is null) return null;

        return new PartnerDto(
            partner.Id, partner.Name, partner.BaseUrl,
            partner.AuthType, partner.IsActive,
            partner.CreatedAt, partner.UpdatedAt);
    }
}

public class GetPartnersHandler(IPartnerRepository partnerRepository)
    : IRequestHandler<GetPartnersQuery, IEnumerable<PartnerDto>>
{
    public async Task<IEnumerable<PartnerDto>> Handle(GetPartnersQuery request, CancellationToken cancellationToken)
    {
        var partners = await partnerRepository.GetAllAsync(cancellationToken);
        return partners.Select(p => new PartnerDto(
            p.Id, p.Name, p.BaseUrl, p.AuthType, p.IsActive, p.CreatedAt, p.UpdatedAt));
    }
}

public class GetFieldMappingsHandler(IFieldMappingRepository fieldMappingRepository)
    : IRequestHandler<GetFieldMappingsQuery, IEnumerable<FieldMappingDto>>
{
    public async Task<IEnumerable<FieldMappingDto>> Handle(GetFieldMappingsQuery request, CancellationToken cancellationToken)
    {
        var mappings = await fieldMappingRepository.GetByPartnerIdAsync(request.PartnerId, cancellationToken);
        return mappings.Select(m => new FieldMappingDto(
            m.Id, m.PartnerId, m.Direction, m.SourceField, m.TargetField, m.ServiceType));
    }
}

public class GetPartnerEndpointsHandler(IPartnerEndpointRepository endpointRepository)
    : IRequestHandler<GetPartnerEndpointsQuery, IEnumerable<PartnerEndpointDto>>
{
    public async Task<IEnumerable<PartnerEndpointDto>> Handle(GetPartnerEndpointsQuery request, CancellationToken cancellationToken)
    {
        var endpoints = await endpointRepository.GetByPartnerIdAsync(request.PartnerId, cancellationToken);
        return endpoints.Select(e => new PartnerEndpointDto(
            e.Id, e.PartnerId, e.ServiceType, e.HttpMethod, e.Path));
    }
}

public class GetServiceOrderHandler(IServiceOrderRepository serviceOrderRepository)
    : IRequestHandler<GetServiceOrderQuery, ServiceOrderDto?>
{
    public async Task<ServiceOrderDto?> Handle(GetServiceOrderQuery request, CancellationToken cancellationToken)
    {
        var order = await serviceOrderRepository.GetByIdAsync(request.Id, cancellationToken);
        if (order is null) return null;

        return new ServiceOrderDto(
            order.Id, order.ExternalId, order.PartnerId, order.Partner?.Name,
            order.ServiceType, order.Status, order.PartnerExternalId,
            order.CreatedAt, order.UpdatedAt);
    }
}

public class GetServiceOrdersHandler(IServiceOrderRepository serviceOrderRepository)
    : IRequestHandler<GetServiceOrdersQuery, IEnumerable<ServiceOrderDto>>
{
    public async Task<IEnumerable<ServiceOrderDto>> Handle(GetServiceOrdersQuery request, CancellationToken cancellationToken)
    {
        var orders = await serviceOrderRepository.GetAllAsync(cancellationToken);
        return orders.Select(o => new ServiceOrderDto(
            o.Id, o.ExternalId, o.PartnerId, o.Partner?.Name,
            o.ServiceType, o.Status, o.PartnerExternalId,
            o.CreatedAt, o.UpdatedAt));
    }
}
