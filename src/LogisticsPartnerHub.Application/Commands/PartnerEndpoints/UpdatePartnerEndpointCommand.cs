using LogisticsPartnerHub.Application.DTOs;
using LogisticsPartnerHub.Domain.Enums;
using MediatR;

namespace LogisticsPartnerHub.Application.Commands.PartnerEndpoints;

public record UpdatePartnerEndpointCommand(
    Guid Id,
    ServiceType ServiceType,
    string HttpMethod,
    string Path) : IRequest<PartnerEndpointDto>;
