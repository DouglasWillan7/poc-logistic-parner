using LogisticsPartnerHub.Application.DTOs;
using LogisticsPartnerHub.Domain.Interfaces.Repositories;
using MediatR;

namespace LogisticsPartnerHub.Application.Commands.FieldMappings;

public class UpdateFieldMappingHandler(
    IFieldMappingRepository fieldMappingRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateFieldMappingCommand, FieldMappingDto>
{
    public async Task<FieldMappingDto> Handle(UpdateFieldMappingCommand request, CancellationToken cancellationToken)
    {
        var mapping = await fieldMappingRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"FieldMapping {request.Id} not found");

        mapping.Direction = request.Direction;
        mapping.SourceField = request.SourceField;
        mapping.TargetField = request.TargetField;
        mapping.ServiceType = request.ServiceType;

        fieldMappingRepository.Update(mapping);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new FieldMappingDto(
            mapping.Id, mapping.PartnerId, mapping.Direction,
            mapping.SourceField, mapping.TargetField, mapping.ServiceType);
    }
}
