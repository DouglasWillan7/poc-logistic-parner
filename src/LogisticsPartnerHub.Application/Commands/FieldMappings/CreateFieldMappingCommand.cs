using LogisticsPartnerHub.Application.DTOs;
using LogisticsPartnerHub.Domain.Enums;
using MediatR;

namespace LogisticsPartnerHub.Application.Commands.FieldMappings;

public record CreateFieldMappingCommand(
    Guid PartnerId,
    MappingDirection Direction,
    string SourceField,
    string TargetField,
    ServiceType ServiceType) : IRequest<FieldMappingDto>;
