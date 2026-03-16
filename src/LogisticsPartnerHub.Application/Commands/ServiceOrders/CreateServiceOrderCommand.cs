using LogisticsPartnerHub.Application.DTOs;
using LogisticsPartnerHub.Domain.Enums;
using MediatR;

namespace LogisticsPartnerHub.Application.Commands.ServiceOrders;

public record CreateServiceOrderCommand(
    string ExternalId,
    Guid PartnerId,
    ServiceType ServiceType,
    string Payload) : IRequest<ServiceOrderDto>;
