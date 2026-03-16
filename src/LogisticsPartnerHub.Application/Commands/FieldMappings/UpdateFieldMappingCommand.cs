using LogisticsPartnerHub.Application.DTOs;
using LogisticsPartnerHub.Domain.Enums;
using MediatR;

namespace LogisticsPartnerHub.Application.Commands.FieldMappings;

public record UpdateFieldMappingCommand(
    Guid Id,
    MappingDirection Direction,
    string SourcePath,
    string TargetPath,
    string? DefaultValue,
    ServiceType ServiceType) : IRequest<FieldMappingDto>;
