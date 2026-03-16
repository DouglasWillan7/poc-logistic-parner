using LogisticsPartnerHub.Application.DTOs;
using LogisticsPartnerHub.Domain.Entities;
using LogisticsPartnerHub.Domain.Interfaces.Repositories;
using MediatR;

namespace LogisticsPartnerHub.Application.Commands.Partners;

public class CreatePartnerHandler(
    IPartnerRepository partnerRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreatePartnerCommand, PartnerDto>
{
    public async Task<PartnerDto> Handle(CreatePartnerCommand request, CancellationToken cancellationToken)
    {
        var partner = new Partner
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            BaseUrl = request.BaseUrl,
            AuthType = request.AuthType,
            AuthConfig = request.AuthConfig,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await partnerRepository.AddAsync(partner, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new PartnerDto(
            partner.Id, partner.Name, partner.BaseUrl,
            partner.AuthType, partner.IsActive,
            partner.CreatedAt, partner.UpdatedAt);
    }
}
