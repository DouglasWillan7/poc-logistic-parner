using LogisticsPartnerHub.Application.DTOs;
using LogisticsPartnerHub.Domain.Enums;
using MediatR;

namespace LogisticsPartnerHub.Application.Commands.Partners;

public record CreatePartnerCommand(
    string Name,
    string BaseUrl,
    AuthType AuthType,
    string AuthConfig) : IRequest<PartnerDto>;
