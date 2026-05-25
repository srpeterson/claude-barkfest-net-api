using Barkfest.Application.Features.Auth.Queries.CheckDisplayName;
using Barkfest.Domain.Interfaces;
using NSubstitute;

namespace Barkfest.Application.Tests.Features.Auth.Queries;

public class CheckDisplayNameQueryHandlerTests
{
    private readonly IOwnerRepository _ownerRepository = Substitute.For<IOwnerRepository>();
    private readonly CheckDisplayNameQueryHandler _checkDisplayNameQueryHandler;

    public CheckDisplayNameQueryHandlerTests()
    {
        _checkDisplayNameQueryHandler = new CheckDisplayNameQueryHandler(_ownerRepository);
    }

    // -----------------------------------------------------------------------
    // Availability
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_When_DisplayNameIsAvailable_Returns_True()
    {
        _ownerRepository.IsDisplayNameAvailableAsync("coolpetdad", CancellationToken.None).Returns(true);

        var result = await _checkDisplayNameQueryHandler.Handle(
            new CheckDisplayNameQuery("Cool Pet Dad"), CancellationToken.None);

        result.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_When_DisplayNameIsNotAvailable_Returns_False()
    {
        _ownerRepository.IsDisplayNameAvailableAsync("coolpetdad", CancellationToken.None).Returns(false);

        var result = await _checkDisplayNameQueryHandler.Handle(
            new CheckDisplayNameQuery("Cool Pet Dad"), CancellationToken.None);

        result.ShouldBeFalse();
    }

    // -----------------------------------------------------------------------
    // Normalisation — spaces and case stripped before repository call
    // -----------------------------------------------------------------------

    [Theory]
    [InlineData("Cool Pet Dad")]
    [InlineData("cool pet dad")]
    [InlineData("COOL PET DAD")]
    [InlineData("CoolPetDad")]
    [InlineData("coolpetdad")]
    public async Task Handle_When_DisplayNameVariants_Passes_SameNormalizedValue(string input)
    {
        _ownerRepository.IsDisplayNameAvailableAsync("coolpetdad", CancellationToken.None).Returns(true);

        await _checkDisplayNameQueryHandler.Handle(
            new CheckDisplayNameQuery(input), CancellationToken.None);

        await _ownerRepository.Received(1).IsDisplayNameAvailableAsync("coolpetdad", CancellationToken.None);
    }

    // -----------------------------------------------------------------------
    // Empty / whitespace — short-circuit without hitting repository
    // -----------------------------------------------------------------------

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_When_ValueIsEmptyOrWhitespace_Returns_True_WithoutQueryingRepository(string value)
    {
        var result = await _checkDisplayNameQueryHandler.Handle(
            new CheckDisplayNameQuery(value), CancellationToken.None);

        result.ShouldBeTrue();
        await _ownerRepository.DidNotReceive().IsDisplayNameAvailableAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}
