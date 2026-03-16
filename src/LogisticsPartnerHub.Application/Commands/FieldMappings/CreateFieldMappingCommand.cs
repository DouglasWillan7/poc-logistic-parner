using LogisticsPartnerHub.Application.DTOs;
using LogisticsPartnerHub.Domain.Enums;
using MediatR;

namespace LogisticsPartnerHub.Application.Commands.FieldMappings;

public record CreateFieldMappingCommand(
    Guid PartnerId,
    MappingDirection Direction,
    string SourcePath,
    string TargetPath,
    string? DefaultValue,
    int Order,
    ServiceType ServiceType) : IRequest<FieldMappingDto>;
