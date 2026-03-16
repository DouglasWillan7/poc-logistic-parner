using LogisticsPartnerHub.Application.DTOs;
using LogisticsPartnerHub.Domain.Interfaces.Repositories;
using MediatR;

namespace LogisticsPartnerHub.Application.Commands.Partners;

public class UpdatePartnerHandler(
    IPartnerRepository partnerRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdatePartnerCommand, PartnerDto>
{
    public async Task<PartnerDto> Handle(UpdatePartnerCommand request, CancellationToken cancellationToken)
    {
        var partner = await partnerRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Partner {request.Id} not found");

        partner.Name = request.Name;
        partner.BaseUrl = request.BaseUrl;
        partner.AuthType = request.AuthType;
        partner.AuthConfig = request.AuthConfig;
        partner.IsActive = request.IsActive;
        partner.UpdatedAt = DateTime.UtcNow;

        partnerRepository.Update(partner);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new PartnerDto(
            partner.Id, partner.Name, partner.BaseUrl,
            partner.AuthType, partner.IsActive,
            partner.CreatedAt, partner.UpdatedAt);
    }
}
