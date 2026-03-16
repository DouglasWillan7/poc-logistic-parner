using LogisticsPartnerHub.Application.DTOs;
using MediatR;

namespace LogisticsPartnerHub.Application.Queries;

public record GetPartnerQuery(Guid Id) : IRequest<PartnerDto?>;
public record GetPartnersQuery : IRequest<IEnumerable<PartnerDto>>;
public record GetFieldMappingsQuery(Guid PartnerId) : IRequest<IEnumerable<FieldMappingDto>>;
public record GetPartnerEndpointsQuery(Guid PartnerId) : IRequest<IEnumerable<PartnerEndpointDto>>;
public record GetServiceOrderQuery(Guid Id) : IRequest<ServiceOrderDto?>;
public record GetServiceOrdersQuery : IRequest<IEnumerable<ServiceOrderDto>>;
