using System.Text.Json;
using FluentAssertions;
using LogisticsPartnerHub.Application.Services;
using LogisticsPartnerHub.Domain.Entities;
using LogisticsPartnerHub.Domain.Enums;
using LogisticsPartnerHub.Domain.Interfaces.Repositories;
using NSubstitute;

namespace LogisticsPartnerHub.Tests.Services;

public class PayloadTransformerServiceTests
{
    private readonly IFieldMappingRepository _fieldMappingRepository = Substitute.For<IFieldMappingRepository>();
    private readonly PayloadTransformerService _sut;

    public PayloadTransformerServiceTests()
    {
        _sut = new PayloadTransformerService(_fieldMappingRepository);
    }

    [Fact]
    public async Task TransformOutboundAsync_ShouldRenameFieldsAccordingToMappings()
    {
        // Arrange
        var partnerId = Guid.NewGuid();
        var mappings = new List<FieldMapping>
        {
            new() { SourceField = "pickup_address", TargetField = "endereco_retirada", Direction = MappingDirection.Outbound },
            new() { SourceField = "vehicle_plate", TargetField = "placa", Direction = MappingDirection.Outbound }
        };

        _fieldMappingRepository
            .GetByPartnerAndServiceTypeAsync(partnerId, ServiceType.Recolhimento, MappingDirection.Outbound, Arg.Any<CancellationToken>())
            .Returns(mappings);

        var payload = JsonSerializer.Serialize(new
        {
            pickup_address = "Rua A, 123",
            vehicle_plate = "ABC1234",
            notes = "Urgent"
        });

        // Act
        var result = await _sut.TransformOutboundAsync(payload, partnerId, ServiceType.Recolhimento);

        // Assert
        var json = JsonDocument.Parse(result).RootElement;
        json.GetProperty("endereco_retirada").GetString().Should().Be("Rua A, 123");
        json.GetProperty("placa").GetString().Should().Be("ABC1234");
        json.GetProperty("notes").GetString().Should().Be("Urgent"); // unmapped field kept
    }

    [Fact]
    public async Task TransformOutboundAsync_WithNoMappings_ShouldKeepOriginalFields()
    {
        // Arrange
        var partnerId = Guid.NewGuid();
        _fieldMappingRepository
            .GetByPartnerAndServiceTypeAsync(partnerId, ServiceType.FreteMoto, MappingDirection.Outbound, Arg.Any<CancellationToken>())
            .Returns(new List<FieldMapping>());

        var payload = JsonSerializer.Serialize(new { field1 = "value1" });

        // Act
        var result = await _sut.TransformOutboundAsync(payload, partnerId, ServiceType.FreteMoto);

        // Assert
        var json = JsonDocument.Parse(result).RootElement;
        json.GetProperty("field1").GetString().Should().Be("value1");
    }

    [Fact]
    public async Task TransformInboundAsync_ShouldApplyReverseMappings()
    {
        // Arrange
        var partnerId = Guid.NewGuid();
        var mappings = new List<FieldMapping>
        {
            new() { SourceField = "estado", TargetField = "status", Direction = MappingDirection.Inbound },
            new() { SourceField = "id_pedido", TargetField = "order_id", Direction = MappingDirection.Inbound }
        };

        _fieldMappingRepository
            .GetByPartnerAndServiceTypeAsync(partnerId, ServiceType.Recolhimento, MappingDirection.Inbound, Arg.Any<CancellationToken>())
            .Returns(mappings);

        var payload = JsonSerializer.Serialize(new
        {
            estado = "Concluido",
            id_pedido = "ORD-001"
        });

        // Act
        var result = await _sut.TransformInboundAsync(payload, partnerId, ServiceType.Recolhimento);

        // Assert
        var json = JsonDocument.Parse(result).RootElement;
        json.GetProperty("status").GetString().Should().Be("Concluido");
        json.GetProperty("order_id").GetString().Should().Be("ORD-001");
    }
}
