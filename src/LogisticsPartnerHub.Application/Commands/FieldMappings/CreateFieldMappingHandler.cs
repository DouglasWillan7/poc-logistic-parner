using LogisticsPartnerHub.Application.DTOs;
using LogisticsPartnerHub.Domain.Entities;
using LogisticsPartnerHub.Domain.Interfaces.Repositories;
using MediatR;

namespace LogisticsPartnerHub.Application.Commands.FieldMappings;

public class CreateFieldMappingHandler(
    IFieldMappingRepository fieldMappingRepository,
    IPartnerRepository partnerRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateFieldMappingCommand, FieldMappingDto>
{
    public async Task<FieldMappingDto> Handle(CreateFieldMappingCommand request, CancellationToken cancellationToken)
    {
        var partner = await partnerRepository.GetByIdAsync(request.PartnerId, cancellationToken)
            ?? throw new KeyNotFoundException($"Partner {request.PartnerId} not found");

        var mapping = new FieldMapping
        {
            Id = Guid.NewGuid(),
            PartnerId = partner.Id,
            Direction = request.Direction,
            SourceField = request.SourceField,
            TargetField = request.TargetField,
            ServiceType = request.ServiceType
        };

        await fieldMappingRepository.AddAsync(mapping, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new FieldMappingDto(
            mapping.Id, mapping.PartnerId, mapping.Direction,
            mapping.SourceField, mapping.TargetField, mapping.ServiceType);
    }
}
