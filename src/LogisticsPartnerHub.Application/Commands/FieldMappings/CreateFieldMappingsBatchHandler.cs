using LogisticsPartnerHub.Application.DTOs;
using LogisticsPartnerHub.Domain.Entities;
using LogisticsPartnerHub.Domain.Enums;
using LogisticsPartnerHub.Domain.Interfaces.Repositories;
using MediatR;

namespace LogisticsPartnerHub.Application.Commands.FieldMappings;

public class CreateFieldMappingsBatchHandler(
    IFieldMappingRepository fieldMappingRepository,
    IPartnerRepository partnerRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateFieldMappingsBatchCommand, IEnumerable<FieldMappingDto>>
{
    public async Task<IEnumerable<FieldMappingDto>> Handle(CreateFieldMappingsBatchCommand request, CancellationToken cancellationToken)
    {
        var partner = await partnerRepository.GetByIdAsync(request.PartnerId, cancellationToken)
            ?? throw new KeyNotFoundException($"Partner {request.PartnerId} not found");

        var existingByKey = new Dictionary<(ServiceType, MappingDirection), int>();

        foreach (var serviceType in request.Mappings.Select(m => m.ServiceType).Distinct())
        {
            foreach (var direction in request.Mappings.Select(m => m.Direction).Distinct())
            {
                var existing = await fieldMappingRepository.GetByPartnerAndServiceTypeAsync(
                    partner.Id, serviceType, direction, cancellationToken);

                var maxOrder = existing.Any() ? existing.Max(m => m.Order) : -1;
                existingByKey[(serviceType, direction)] = maxOrder;
            }
        }

        var mappings = new List<FieldMapping>();

        foreach (var item in request.Mappings)
        {
            var key = (item.ServiceType, item.Direction);
            existingByKey[key]++;

            mappings.Add(new FieldMapping
            {
                Id = Guid.NewGuid(),
                PartnerId = partner.Id,
                Direction = item.Direction,
                SourcePath = item.SourcePath,
                TargetPath = item.TargetPath,
                DefaultValue = item.DefaultValue,
                Order = existingByKey[key],
                ServiceType = item.ServiceType
            });
        }

        await fieldMappingRepository.AddRangeAsync(mappings, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return mappings.Select(m => new FieldMappingDto(
            m.Id, m.PartnerId, m.Direction,
            m.SourcePath, m.TargetPath, m.DefaultValue,
            m.Order, m.ServiceType));
    }
}
