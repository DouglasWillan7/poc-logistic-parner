using LogisticsPartnerHub.Application.DTOs;
using LogisticsPartnerHub.Domain.Enums;
using MediatR;

namespace LogisticsPartnerHub.Application.Commands.FieldMappings;

public record FieldMappingItem(
    MappingDirection Direction,
    string SourcePath,
    string TargetPath,
    string? DefaultValue,
    ServiceType ServiceType);

public record CreateFieldMappingsBatchCommand(
    Guid PartnerId,
    List<FieldMappingItem> Mappings) : IRequest<IEnumerable<FieldMappingDto>>;
