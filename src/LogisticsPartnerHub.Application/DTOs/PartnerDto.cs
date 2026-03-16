using LogisticsPartnerHub.Domain.Enums;

namespace LogisticsPartnerHub.Application.DTOs;

public record PartnerDto(
    Guid Id,
    string Name,
    string BaseUrl,
    AuthType AuthType,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record FieldMappingDto(
    Guid Id,
    Guid PartnerId,
    MappingDirection Direction,
    string SourcePath,
    string TargetPath,
    string? DefaultValue,
    int Order,
    ServiceType ServiceType);

public record PartnerEndpointDto(
    Guid Id,
    Guid PartnerId,
    ServiceType ServiceType,
    string HttpMethod,
    string Path);

public record ServiceOrderDto(
    Guid Id,
    string ExternalId,
    Guid PartnerId,
    string? PartnerName,
    ServiceType ServiceType,
    ServiceOrderStatus Status,
    string? PartnerExternalId,
    DateTime CreatedAt,
    DateTime UpdatedAt);
