using FluentAssertions;
using LogisticsPartnerHub.Application.Commands.ServiceOrders;
using LogisticsPartnerHub.Domain.Entities;
using LogisticsPartnerHub.Domain.Enums;
using LogisticsPartnerHub.Domain.Interfaces.Repositories;
using NSubstitute;

namespace LogisticsPartnerHub.Tests.Handlers;

public class CreateServiceOrderHandlerTests
{
    private readonly IServiceOrderRepository _serviceOrderRepository = Substitute.For<IServiceOrderRepository>();
    private readonly IPartnerRepository _partnerRepository = Substitute.For<IPartnerRepository>();
    private readonly IFieldMappingRepository _fieldMappingRepository = Substitute.For<IFieldMappingRepository>();
    private readonly IPartnerEndpointRepository _endpointRepository = Substitute.For<IPartnerEndpointRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly CreateServiceOrderHandler _sut;

    public CreateServiceOrderHandlerTests()
    {
        _sut = new CreateServiceOrderHandler(
            _serviceOrderRepository, _partnerRepository,
            _fieldMappingRepository, _endpointRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_ShouldCreateServiceOrder_WhenPartnerIsActiveAndConfigured()
    {
        // Arrange
        var partnerId = Guid.NewGuid();
        var partner = new Partner { Id = partnerId, Name = "Test Partner", IsActive = true };
        var command = new CreateServiceOrderCommand("EXT-001", partnerId, ServiceType.Recolhimento, """{"field":"value"}""");

        _serviceOrderRepository.GetByExternalIdAsync("EXT-001", Arg.Any<CancellationToken>()).Returns((ServiceOrder?)null);
        _partnerRepository.GetByIdAsync(partnerId, Arg.Any<CancellationToken>()).Returns(partner);
        _fieldMappingRepository.GetByPartnerAndServiceTypeAsync(partnerId, ServiceType.Recolhimento, MappingDirection.Outbound, Arg.Any<CancellationToken>())
            .Returns(new List<FieldMapping> { new() { SourceField = "field", TargetField = "campo" } });
        _endpointRepository.GetByPartnerAndServiceTypeAsync(partnerId, ServiceType.Recolhimento, Arg.Any<CancellationToken>())
            .Returns(new PartnerEndpoint { Path = "/api/pickups", HttpMethod = "POST" });
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.ExternalId.Should().Be("EXT-001");
        result.Status.Should().Be(ServiceOrderStatus.Solicitado);
        result.PartnerId.Should().Be(partnerId);
        await _serviceOrderRepository.Received(1).AddAsync(Arg.Any<ServiceOrder>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnExisting_WhenDuplicateExternalId()
    {
        // Arrange
        var partnerId = Guid.NewGuid();
        var existing = new ServiceOrder
        {
            Id = Guid.NewGuid(),
            ExternalId = "EXT-001",
            PartnerId = partnerId,
            ServiceType = ServiceType.Recolhimento,
            Status = ServiceOrderStatus.Aceito,
            Partner = new Partner { Name = "Test" },
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var command = new CreateServiceOrderCommand("EXT-001", partnerId, ServiceType.Recolhimento, """{"field":"value"}""");
        _serviceOrderRepository.GetByExternalIdAsync("EXT-001", Arg.Any<CancellationToken>()).Returns(existing);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Id.Should().Be(existing.Id);
        result.Status.Should().Be(ServiceOrderStatus.Aceito);
        await _serviceOrderRepository.DidNotReceive().AddAsync(Arg.Any<ServiceOrder>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenPartnerIsInactive()
    {
        // Arrange
        var partnerId = Guid.NewGuid();
        var partner = new Partner { Id = partnerId, Name = "Inactive", IsActive = false };
        var command = new CreateServiceOrderCommand("EXT-002", partnerId, ServiceType.Recolhimento, "{}");

        _serviceOrderRepository.GetByExternalIdAsync("EXT-002", Arg.Any<CancellationToken>()).Returns((ServiceOrder?)null);
        _partnerRepository.GetByIdAsync(partnerId, Arg.Any<CancellationToken>()).Returns(partner);

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*not active*");
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenNoFieldMappingsConfigured()
    {
        // Arrange
        var partnerId = Guid.NewGuid();
        var partner = new Partner { Id = partnerId, Name = "Test", IsActive = true };
        var command = new CreateServiceOrderCommand("EXT-003", partnerId, ServiceType.FreteMoto, "{}");

        _serviceOrderRepository.GetByExternalIdAsync("EXT-003", Arg.Any<CancellationToken>()).Returns((ServiceOrder?)null);
        _partnerRepository.GetByIdAsync(partnerId, Arg.Any<CancellationToken>()).Returns(partner);
        _fieldMappingRepository.GetByPartnerAndServiceTypeAsync(partnerId, ServiceType.FreteMoto, MappingDirection.Outbound, Arg.Any<CancellationToken>())
            .Returns(new List<FieldMapping>());

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*No outbound field mappings*");
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenNoEndpointConfigured()
    {
        // Arrange
        var partnerId = Guid.NewGuid();
        var partner = new Partner { Id = partnerId, Name = "Test", IsActive = true };
        var command = new CreateServiceOrderCommand("EXT-004", partnerId, ServiceType.FretePecas, "{}");

        _serviceOrderRepository.GetByExternalIdAsync("EXT-004", Arg.Any<CancellationToken>()).Returns((ServiceOrder?)null);
        _partnerRepository.GetByIdAsync(partnerId, Arg.Any<CancellationToken>()).Returns(partner);
        _fieldMappingRepository.GetByPartnerAndServiceTypeAsync(partnerId, ServiceType.FretePecas, MappingDirection.Outbound, Arg.Any<CancellationToken>())
            .Returns(new List<FieldMapping> { new() { SourceField = "a", TargetField = "b" } });
        _endpointRepository.GetByPartnerAndServiceTypeAsync(partnerId, ServiceType.FretePecas, Arg.Any<CancellationToken>())
            .Returns((PartnerEndpoint?)null);

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*No endpoint configured*");
    }
}
