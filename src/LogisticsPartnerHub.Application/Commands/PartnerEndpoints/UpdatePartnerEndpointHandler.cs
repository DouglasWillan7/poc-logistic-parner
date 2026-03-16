using LogisticsPartnerHub.Application.DTOs;
using LogisticsPartnerHub.Domain.Interfaces.Repositories;
using MediatR;

namespace LogisticsPartnerHub.Application.Commands.PartnerEndpoints;

public class UpdatePartnerEndpointHandler(
    IPartnerEndpointRepository endpointRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdatePartnerEndpointCommand, PartnerEndpointDto>
{
    public async Task<PartnerEndpointDto> Handle(UpdatePartnerEndpointCommand request, CancellationToken cancellationToken)
    {
        var endpoint = await endpointRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"PartnerEndpoint {request.Id} not found");

        endpoint.ServiceType = request.ServiceType;
        endpoint.HttpMethod = request.HttpMethod;
        endpoint.Path = request.Path;

        endpointRepository.Update(endpoint);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new PartnerEndpointDto(
            endpoint.Id, endpoint.PartnerId, endpoint.ServiceType,
            endpoint.HttpMethod, endpoint.Path);
    }
}
