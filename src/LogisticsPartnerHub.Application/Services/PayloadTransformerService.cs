using System.Text.Json;
using System.Text.Json.Nodes;
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

        var source = JsonNode.Parse(canonicalPayload) ?? new JsonObject();
        var result = new JsonObject();

        foreach (var mapping in mappings)
        {
            var value = JsonPathBuilder.ExtractValue(source, mapping.SourcePath);

            if (value is null && mapping.DefaultValue is not null)
            {
                value = JsonPathBuilder.ParseDefaultValue(mapping.DefaultValue);
            }

            if (value is not null)
            {
                JsonPathBuilder.SetValue(result, mapping.TargetPath, value);
            }
        }

        return result.ToJsonString(new JsonSerializerOptions { WriteIndented = false });
    }

    public async Task<string> TransformInboundAsync(
        string partnerPayload, Guid partnerId, ServiceType serviceType,
        CancellationToken cancellationToken = default)
    {
        var mappings = await fieldMappingRepository.GetByPartnerAndServiceTypeAsync(
            partnerId, serviceType, MappingDirection.Inbound, cancellationToken);

        var source = JsonNode.Parse(partnerPayload) ?? new JsonObject();
        var result = new JsonObject();

        foreach (var mapping in mappings)
        {
            var value = JsonPathBuilder.ExtractValue(source, mapping.SourcePath);

            if (value is null && mapping.DefaultValue is not null)
            {
                value = JsonPathBuilder.ParseDefaultValue(mapping.DefaultValue);
            }

            if (value is not null)
            {
                JsonPathBuilder.SetValue(result, mapping.TargetPath, value);
            }
        }

        return result.ToJsonString(new JsonSerializerOptions { WriteIndented = false });
    }
}
