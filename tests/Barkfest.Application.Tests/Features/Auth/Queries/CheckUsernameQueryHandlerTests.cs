using Barkfest.Application.Features.Auth.Queries.CheckUsername;
using Barkfest.Domain.Interfaces;
using NSubstitute;

namespace Barkfest.Application.Tests.Features.Auth.Queries;

public class CheckUsernameQueryHandlerTests
{
    private readonly IOwnerRepository _ownerRepository = Substitute.For<IOwnerRepository>();
    private readonly CheckUsernameQueryHandler _checkUsernameQueryHandler;

    public CheckUsernameQueryHandlerTests()
    {
        _checkUsernameQueryHandler = new CheckUsernameQueryHandler(_ownerRepository);
    }

    // -----------------------------------------------------------------------
    // Availability
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_When_UsernameIsAvailable_Returns_True()
    {
        _ownerRepository.IsUsernameAvailableAsync("johndoe", CancellationToken.None).Returns(true);

        var result = await _checkUsernameQueryHandler.Handle(
            new CheckUsernameQuery("johndoe"), CancellationToken.None);

        result.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_When_UsernameIsNotAvailable_Returns_False()
    {
        _ownerRepository.IsUsernameAvailableAsync("johndoe", CancellationToken.None).Returns(false);

        var result = await _checkUsernameQueryHandler.Handle(
            new CheckUsernameQuery("johndoe"), CancellationToken.None);

        result.ShouldBeFalse();
    }

    // -----------------------------------------------------------------------
    // Trimming — surrounding whitespace stripped before repository call
    // -----------------------------------------------------------------------

    [Theory]
    [InlineData("johndoe")]
    [InlineData("  johndoe")]
    [InlineData("johndoe  ")]
    [InlineData("  johndoe  ")]
    public async Task Handle_When_UsernameHasSurroundingWhitespace_Passes_TrimmedValue(string input)
    {
        _ownerRepository.IsUsernameAvailableAsync("johndoe", CancellationToken.None).Returns(true);

        await _checkUsernameQueryHandler.Handle(
            new CheckUsernameQuery(input), CancellationToken.None);

        await _ownerRepository.Received(1).IsUsernameAvailableAsync("johndoe", CancellationToken.None);
    }

    // -----------------------------------------------------------------------
    // Empty / whitespace — short-circuit without hitting repository
    // -----------------------------------------------------------------------

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_When_ValueIsEmptyOrWhitespace_Returns_True_WithoutQueryingRepository(string value)
    {
        var result = await _checkUsernameQueryHandler.Handle(
            new CheckUsernameQuery(value), CancellationToken.None);

        result.ShouldBeTrue();
        await _ownerRepository.DidNotReceive().IsUsernameAvailableAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}
