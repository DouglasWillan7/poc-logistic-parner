using FluentAssertions;
using LogisticsPartnerHub.Application.Commands.ServiceOrders;
using LogisticsPartnerHub.Application.Interfaces;
using LogisticsPartnerHub.Domain.Entities;
using LogisticsPartnerHub.Domain.Enums;
using LogisticsPartnerHub.Domain.Interfaces.Repositories;
using LogisticsPartnerHub.Domain.Interfaces.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace LogisticsPartnerHub.Tests.Handlers;

public class ProcessServiceOrderHandlerTests
{
    private readonly IServiceOrderRepository _serviceOrderRepository = Substitute.For<IServiceOrderRepository>();
    private readonly IPartnerRepository _partnerRepository = Substitute.For<IPartnerRepository>();
    private readonly IPartnerEndpointRepository _endpointRepository = Substitute.For<IPartnerEndpointRepository>();
    private readonly IPayloadTransformer _payloadTransformer = Substitute.For<IPayloadTransformer>();
    private readonly IPartnerHttpClient _partnerHttpClient = Substitute.For<IPartnerHttpClient>();
    private readonly IServiceOrderLogRepository _logRepository = Substitute.For<IServiceOrderLogRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ProcessServiceOrderHandler _sut;

    public ProcessServiceOrderHandlerTests()
    {
        _sut = new ProcessServiceOrderHandler(
            _serviceOrderRepository, _partnerRepository, _endpointRepository,
            _payloadTransformer, _partnerHttpClient, _logRepository, _unitOfWork,
            Substitute.For<ILogger<ProcessServiceOrderHandler>>());
    }

    [Fact]
    public async Task Handle_ShouldTransformAndSendToPartner_WhenSuccess()
    {
        // Arrange
        var partnerId = Guid.NewGuid();
        var orderId = Guid.NewGuid();

        var serviceOrder = new ServiceOrder
        {
            Id = orderId,
            PartnerId = partnerId,
            ServiceType = ServiceType.Recolhimento,
            CanonicalPayload = """{"pickup_address":"Rua A"}""",
            Status = ServiceOrderStatus.Solicitado,
            Logs = new List<ServiceOrderLog>()
        };
        var partner = new Partner { Id = partnerId, Name = "Partner A", BaseUrl = "https://api.partner.com" };
        var endpoint = new PartnerEndpoint { Path = "/pickups", HttpMethod = "POST" };

        _serviceOrderRepository.GetByIdAsync(orderId, Arg.Any<CancellationToken>()).Returns(serviceOrder);
        _partnerRepository.GetByIdAsync(partnerId, Arg.Any<CancellationToken>()).Returns(partner);
        _endpointRepository.GetByPartnerAndServiceTypeAsync(partnerId, ServiceType.Recolhimento, Arg.Any<CancellationToken>()).Returns(endpoint);
        _payloadTransformer.TransformOutboundAsync(Arg.Any<string>(), partnerId, ServiceType.Recolhimento, Arg.Any<CancellationToken>())
            .Returns("""{"endereco_retirada":"Rua A"}""");
        _partnerHttpClient.SendAsync(partner, "POST", "https://api.partner.com/pickups", Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((200, """{"id":"P-001"}"""));

        // Act
        var result = await _sut.Handle(new ProcessServiceOrderCommand(orderId), CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        serviceOrder.Status.Should().Be(ServiceOrderStatus.Aceito);
        serviceOrder.PartnerPayload.Should().NotBeNull();
        await _logRepository.Received(1).AddAsync(Arg.Any<ServiceOrderLog>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnFalse_WhenPartnerReturnsError()
    {
        // Arrange
        var partnerId = Guid.NewGuid();
        var orderId = Guid.NewGuid();

        var serviceOrder = new ServiceOrder
        {
            Id = orderId,
            PartnerId = partnerId,
            ServiceType = ServiceType.FreteMoto,
            CanonicalPayload = "{}",
            Status = ServiceOrderStatus.Solicitado,
            Logs = new List<ServiceOrderLog>()
        };
        var partner = new Partner { Id = partnerId, Name = "Partner B", BaseUrl = "https://api.b.com" };
        var endpoint = new PartnerEndpoint { Path = "/freight", HttpMethod = "POST" };

        _serviceOrderRepository.GetByIdAsync(orderId, Arg.Any<CancellationToken>()).Returns(serviceOrder);
        _partnerRepository.GetByIdAsync(partnerId, Arg.Any<CancellationToken>()).Returns(partner);
        _endpointRepository.GetByPartnerAndServiceTypeAsync(partnerId, ServiceType.FreteMoto, Arg.Any<CancellationToken>()).Returns(endpoint);
        _payloadTransformer.TransformOutboundAsync(Arg.Any<string>(), partnerId, ServiceType.FreteMoto, Arg.Any<CancellationToken>()).Returns("{}");
        _partnerHttpClient.SendAsync(partner, "POST", Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((500, """{"error":"Internal Server Error"}"""));

        // Act
        var result = await _sut.Handle(new ProcessServiceOrderCommand(orderId), CancellationToken.None);

        // Assert
        result.Should().BeFalse();
        serviceOrder.Status.Should().Be(ServiceOrderStatus.Solicitado); // status não mudou
    }
}
