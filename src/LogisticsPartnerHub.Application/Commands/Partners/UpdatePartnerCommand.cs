using LogisticsPartnerHub.Application.DTOs;
using LogisticsPartnerHub.Domain.Enums;
using MediatR;

namespace LogisticsPartnerHub.Application.Commands.Partners;

public record UpdatePartnerCommand(
    Guid Id,
    string Name,
    string BaseUrl,
    AuthType AuthType,
    string AuthConfig,
    bool IsActive) : IRequest<PartnerDto>;
