using LogisticsPartnerHub.Application.DTOs;
using LogisticsPartnerHub.Domain.Entities;
using LogisticsPartnerHub.Domain.Interfaces.Repositories;
using MediatR;

namespace LogisticsPartnerHub.Application.Commands.PartnerEndpoints;

public class CreatePartnerEndpointHandler(
    IPartnerEndpointRepository endpointRepository,
    IPartnerRepository partnerRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreatePartnerEndpointCommand, PartnerEndpointDto>
{
    public async Task<PartnerEndpointDto> Handle(CreatePartnerEndpointCommand request, CancellationToken cancellationToken)
    {
        _ = await partnerRepository.GetByIdAsync(request.PartnerId, cancellationToken)
            ?? throw new KeyNotFoundException($"Partner {request.PartnerId} not found");

        var endpoint = new PartnerEndpoint
        {
            Id = Guid.NewGuid(),
            PartnerId = request.PartnerId,
            ServiceType = request.ServiceType,
            HttpMethod = request.HttpMethod,
            Path = request.Path
        };

        await endpointRepository.AddAsync(endpoint, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new PartnerEndpointDto(
            endpoint.Id, endpoint.PartnerId, endpoint.ServiceType,
            endpoint.HttpMethod, endpoint.Path);
    }
}
