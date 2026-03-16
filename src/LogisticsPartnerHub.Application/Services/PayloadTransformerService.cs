using System.Text.Json;
using LogisticsPartnerHub.Domain.Enums;
using LogisticsPartnerHub.Domain.Interfaces.Repositories;
using LogisticsPartnerHub.Domain.Interfaces.Services;

namespace LogisticsPartnerHub.Application.Services;

public class PayloadTransformerService(IFieldMappingRepository fieldMappingRepository) : IPayloadTransformer
{
    public async Task<string> TransformOutboundAsync(
        string canonicalPayload, Guid partnerId, ServiceType serviceType,
        CancellationToken cancellationToken = default)
    {
        var mappings = await fieldMappingRepository.GetByPartnerAndServiceTypeAsync(
            partnerId, serviceType, MappingDirection.Outbound, cancellationToken);

        var sourceJson = JsonDocument.Parse(canonicalPayload);
        var result = new Dictionary<string, JsonElement>();

        var mappingDict = mappings.ToDictionary(m => m.SourceField, m => m.TargetField);

        foreach (var property in sourceJson.RootElement.EnumerateObject())
        {
            if (mappingDict.TryGetValue(property.Name, out var targetField))
            {
                result[targetField] = property.Value;
            }
            else
            {
                // Campos sem mapeamento são mantidos com o nome original
                result[property.Name] = property.Value;
            }
        }

        return JsonSerializer.Serialize(result);
    }

    public async Task<string> TransformInboundAsync(
        string partnerPayload, Guid partnerId, ServiceType serviceType,
        CancellationToken cancellationToken = default)
    {
        var mappings = await fieldMappingRepository.GetByPartnerAndServiceTypeAsync(
            partnerId, serviceType, MappingDirection.Inbound, cancellationToken);

        var sourceJson = JsonDocument.Parse(partnerPayload);
        var result = new Dictionary<string, JsonElement>();

        var mappingDict = mappings.ToDictionary(m => m.SourceField, m => m.TargetField);

        foreach (var property in sourceJson.RootElement.EnumerateObject())
        {
            if (mappingDict.TryGetValue(property.Name, out var targetField))
            {
                result[targetField] = property.Value;
            }
            else
            {
                result[property.Name] = property.Value;
            }
        }

        return JsonSerializer.Serialize(result);
    }
}
