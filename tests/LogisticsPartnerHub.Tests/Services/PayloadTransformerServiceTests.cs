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
    public async Task TransformOutboundAsync_ShouldMapFlatFieldsUsingJsonPath()
    {
        // Arrange
        var partnerId = Guid.NewGuid();
        var mappings = new List<FieldMapping>
        {
            new() { SourcePath = "$.pickup_address", TargetPath = "$.endereco_retirada", Order = 0, Direction = MappingDirection.Outbound },
            new() { SourcePath = "$.vehicle_plate", TargetPath = "$.placa", Order = 1, Direction = MappingDirection.Outbound }
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
    }

    [Fact]
    public async Task TransformOutboundAsync_ShouldBuildNestedObjects()
    {
        // Arrange
        var partnerId = Guid.NewGuid();
        var mappings = new List<FieldMapping>
        {
            new() { SourcePath = "$.client_name", TargetPath = "$.cliente.nome", Order = 0, Direction = MappingDirection.Outbound },
            new() { SourcePath = "$.client_lastname", TargetPath = "$.cliente.sobrenome", Order = 1, Direction = MappingDirection.Outbound },
            new() { SourcePath = "$.client_phone", TargetPath = "$.cliente.telefoneCelular", Order = 2, Direction = MappingDirection.Outbound },
            new() { SourcePath = "$.vehicle_brand", TargetPath = "$.veiculoCliente.marca", Order = 3, Direction = MappingDirection.Outbound },
            new() { SourcePath = "$.vehicle_model", TargetPath = "$.veiculoCliente.modelo", Order = 4, Direction = MappingDirection.Outbound }
        };

        _fieldMappingRepository
            .GetByPartnerAndServiceTypeAsync(partnerId, ServiceType.Recolhimento, MappingDirection.Outbound, Arg.Any<CancellationToken>())
            .Returns(mappings);

        var payload = JsonSerializer.Serialize(new
        {
            client_name = "henrique",
            client_lastname = "rezende",
            client_phone = "31888888111",
            vehicle_brand = "Jeep",
            vehicle_model = "Renegade"
        });

        // Act
        var result = await _sut.TransformOutboundAsync(payload, partnerId, ServiceType.Recolhimento);

        // Assert
        var json = JsonDocument.Parse(result).RootElement;
        json.GetProperty("cliente").GetProperty("nome").GetString().Should().Be("henrique");
        json.GetProperty("cliente").GetProperty("sobrenome").GetString().Should().Be("rezende");
        json.GetProperty("cliente").GetProperty("telefoneCelular").GetString().Should().Be("31888888111");
        json.GetProperty("veiculoCliente").GetProperty("marca").GetString().Should().Be("Jeep");
        json.GetProperty("veiculoCliente").GetProperty("modelo").GetString().Should().Be("Renegade");
    }

    [Fact]
    public async Task TransformOutboundAsync_ShouldUseDefaultValueWhenSourceIsNull()
    {
        // Arrange
        var partnerId = Guid.NewGuid();
        var mappings = new List<FieldMapping>
        {
            new() { SourcePath = "$.scheduled", TargetPath = "$.agendado", DefaultValue = "false", Order = 0, Direction = MappingDirection.Outbound },
            new() { SourcePath = "$.service_type", TargetPath = "$.tipoServico", DefaultValue = "\"REBOQUE\"", Order = 1, Direction = MappingDirection.Outbound }
        };

        _fieldMappingRepository
            .GetByPartnerAndServiceTypeAsync(partnerId, ServiceType.Recolhimento, MappingDirection.Outbound, Arg.Any<CancellationToken>())
            .Returns(mappings);

        var payload = "{}";

        // Act
        var result = await _sut.TransformOutboundAsync(payload, partnerId, ServiceType.Recolhimento);

        // Assert
        var json = JsonDocument.Parse(result).RootElement;
        json.GetProperty("agendado").GetBoolean().Should().BeFalse();
        json.GetProperty("tipoServico").GetString().Should().Be("REBOQUE");
    }

    [Fact]
    public async Task TransformOutboundAsync_WithNoMappings_ShouldReturnEmptyObject()
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
        result.Should().Be("{}");
    }

    [Fact]
    public async Task TransformOutboundAsync_ShouldBuildComplexPayloadLikeSoonApi()
    {
        // Arrange
        var partnerId = Guid.NewGuid();
        var mappings = new List<FieldMapping>
        {
            new() { SourcePath = "$.scheduled", TargetPath = "$.agendado", Order = 0, Direction = MappingDirection.Outbound },
            new() { SourcePath = "$.protocol", TargetPath = "$.ownId", Order = 1, Direction = MappingDirection.Outbound },
            new() { SourcePath = "$.ride_value", TargetPath = "$.valorCorrida", Order = 2, Direction = MappingDirection.Outbound },
            new() { SourcePath = "$.client_name", TargetPath = "$.cliente.nome", Order = 3, Direction = MappingDirection.Outbound },
            new() { SourcePath = "$.client_lastname", TargetPath = "$.cliente.sobrenome", Order = 4, Direction = MappingDirection.Outbound },
            new() { SourcePath = "$.client_phone", TargetPath = "$.cliente.telefoneCelular", Order = 5, Direction = MappingDirection.Outbound },
            new() { SourcePath = "$.vehicle_brand", TargetPath = "$.veiculoCliente.marca", Order = 6, Direction = MappingDirection.Outbound },
            new() { SourcePath = "$.vehicle_type", TargetPath = "$.veiculoCliente.tipoVeiculo", Order = 7, Direction = MappingDirection.Outbound },
            new() { SourcePath = "$.vehicle_model", TargetPath = "$.veiculoCliente.modelo", Order = 8, Direction = MappingDirection.Outbound },
            new() { SourcePath = "$.vehicle_plate", TargetPath = "$.veiculoCliente.placa", Order = 9, Direction = MappingDirection.Outbound },
            new() { SourcePath = "$.service_type_code", TargetPath = "$.tipoServico", Order = 10, Direction = MappingDirection.Outbound },
            new() { SourcePath = "$.origin_address", TargetPath = "$.enderecoOrigem.logradouro", Order = 11, Direction = MappingDirection.Outbound },
            new() { SourcePath = "$.origin_number", TargetPath = "$.enderecoOrigem.numero", Order = 12, Direction = MappingDirection.Outbound },
            new() { SourcePath = "$.origin_lat", TargetPath = "$.enderecoOrigem.latitude", Order = 13, Direction = MappingDirection.Outbound },
            new() { SourcePath = "$.origin_lng", TargetPath = "$.enderecoOrigem.longitude", Order = 14, Direction = MappingDirection.Outbound },
            new() { SourcePath = "$.is_capsized", TargetPath = "$.situacaoVeiculo.capotado", DefaultValue = "false", Order = 15, Direction = MappingDirection.Outbound },
            new() { SourcePath = "$.is_garage", TargetPath = "$.situacaoVeiculo.garagem", DefaultValue = "false", Order = 16, Direction = MappingDirection.Outbound },
        };

        _fieldMappingRepository
            .GetByPartnerAndServiceTypeAsync(partnerId, ServiceType.Recolhimento, MappingDirection.Outbound, Arg.Any<CancellationToken>())
            .Returns(mappings);

        var payload = JsonSerializer.Serialize(new
        {
            scheduled = false,
            protocol = "548478548725777",
            ride_value = "412",
            client_name = "henrique",
            client_lastname = "rezende",
            client_phone = "31888888111",
            vehicle_brand = "Jeep",
            vehicle_type = "SUV",
            vehicle_model = "Renegade",
            vehicle_plate = "PZA-4459",
            service_type_code = "REBOQUE",
            origin_address = "Rua Pamplona - Jardim Paulista, Sao Paulo - SP, Brasil",
            origin_number = "1551",
            origin_lat = -23.570193,
            origin_lng = -46.65962
        });

        // Act
        var result = await _sut.TransformOutboundAsync(payload, partnerId, ServiceType.Recolhimento);

        // Assert
        var json = JsonDocument.Parse(result).RootElement;

        json.GetProperty("agendado").GetBoolean().Should().BeFalse();
        json.GetProperty("ownId").GetString().Should().Be("548478548725777");
        json.GetProperty("valorCorrida").GetString().Should().Be("412");

        json.GetProperty("cliente").GetProperty("nome").GetString().Should().Be("henrique");
        json.GetProperty("cliente").GetProperty("sobrenome").GetString().Should().Be("rezende");
        json.GetProperty("cliente").GetProperty("telefoneCelular").GetString().Should().Be("31888888111");

        json.GetProperty("veiculoCliente").GetProperty("marca").GetString().Should().Be("Jeep");
        json.GetProperty("veiculoCliente").GetProperty("tipoVeiculo").GetString().Should().Be("SUV");
        json.GetProperty("veiculoCliente").GetProperty("modelo").GetString().Should().Be("Renegade");
        json.GetProperty("veiculoCliente").GetProperty("placa").GetString().Should().Be("PZA-4459");

        json.GetProperty("tipoServico").GetString().Should().Be("REBOQUE");

        json.GetProperty("enderecoOrigem").GetProperty("logradouro").GetString().Should().Be("Rua Pamplona - Jardim Paulista, Sao Paulo - SP, Brasil");
        json.GetProperty("enderecoOrigem").GetProperty("numero").GetString().Should().Be("1551");
        json.GetProperty("enderecoOrigem").GetProperty("latitude").GetDouble().Should().Be(-23.570193);
        json.GetProperty("enderecoOrigem").GetProperty("longitude").GetDouble().Should().Be(-46.65962);

        json.GetProperty("situacaoVeiculo").GetProperty("capotado").GetBoolean().Should().BeFalse();
        json.GetProperty("situacaoVeiculo").GetProperty("garagem").GetBoolean().Should().BeFalse();
    }

    [Fact]
    public async Task TransformInboundAsync_ShouldExtractFromNestedObjects()
    {
        // Arrange
        var partnerId = Guid.NewGuid();
        var mappings = new List<FieldMapping>
        {
            new() { SourcePath = "$.dados.estado", TargetPath = "$.status", Order = 0, Direction = MappingDirection.Inbound },
            new() { SourcePath = "$.dados.identificador", TargetPath = "$.order_id", Order = 1, Direction = MappingDirection.Inbound }
        };

        _fieldMappingRepository
            .GetByPartnerAndServiceTypeAsync(partnerId, ServiceType.Recolhimento, MappingDirection.Inbound, Arg.Any<CancellationToken>())
            .Returns(mappings);

        var payload = JsonSerializer.Serialize(new
        {
            dados = new
            {
                estado = "Concluido",
                identificador = "ORD-001"
            }
        });

        // Act
        var result = await _sut.TransformInboundAsync(payload, partnerId, ServiceType.Recolhimento);

        // Assert
        var json = JsonDocument.Parse(result).RootElement;
        json.GetProperty("status").GetString().Should().Be("Concluido");
        json.GetProperty("order_id").GetString().Should().Be("ORD-001");
    }
}
