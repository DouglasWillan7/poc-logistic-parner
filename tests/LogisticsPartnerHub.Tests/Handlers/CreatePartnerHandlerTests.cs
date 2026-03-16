using FluentAssertions;
using LogisticsPartnerHub.Application.Commands.Partners;
using LogisticsPartnerHub.Domain.Entities;
using LogisticsPartnerHub.Domain.Enums;
using LogisticsPartnerHub.Domain.Interfaces.Repositories;
using NSubstitute;

namespace LogisticsPartnerHub.Tests.Handlers;

public class CreatePartnerHandlerTests
{
    private readonly IPartnerRepository _partnerRepository = Substitute.For<IPartnerRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly CreatePartnerHandler _sut;

    public CreatePartnerHandlerTests()
    {
        _sut = new CreatePartnerHandler(_partnerRepository, _unitOfWork);
    }

    [Fact]
    public async Task Handle_ShouldCreatePartnerAndReturnDto()
    {
        // Arrange
        var command = new CreatePartnerCommand(
            "Parceiro Teste",
            "https://api.parceiro.com",
            AuthType.ApiKey,
            """{"apiKey": "test-key"}""");

        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Name.Should().Be("Parceiro Teste");
        result.BaseUrl.Should().Be("https://api.parceiro.com");
        result.AuthType.Should().Be(AuthType.ApiKey);
        result.IsActive.Should().BeTrue();
        result.Id.Should().NotBeEmpty();

        await _partnerRepository.Received(1).AddAsync(Arg.Any<Partner>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
