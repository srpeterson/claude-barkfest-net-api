using Barkfest.Application.Common.Interfaces;
using Barkfest.Application.Features.Auth.Commands.Login;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Errors;
using Barkfest.Domain.Interfaces;
using NSubstitute;

namespace Barkfest.Application.Tests.Features.Auth.Commands;

public class LoginCommandHandlerTests
{
    private readonly IOwnerRepository _ownerRepository = Substitute.For<IOwnerRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly IJwtTokenService _jwtTokenService = Substitute.For<IJwtTokenService>();
    private readonly LoginCommandHandler _loginCommandHandler;

    public LoginCommandHandlerTests()
    {
        _loginCommandHandler = new LoginCommandHandler(_ownerRepository, _passwordHasher, _jwtTokenService);
    }

    [Fact]
    public async Task Handle_When_CredentialsAreValid_Returns_AuthTokenDto()
    {
        var owner = new OwnerBuilder().WithUsername("alice").Build();
        _ownerRepository.GetByUsernameAsync("alice", CancellationToken.None).Returns(owner);
        _passwordHasher.Verify("pass123", owner.PasswordHash).Returns(true);
        _jwtTokenService.GenerateOwnerToken(owner).Returns("jwt-token");
        _jwtTokenService.GetExpiry().Returns(DateTime.UtcNow.AddHours(1));

        var command = new LoginCommand("alice", "pass123");

        var result = await _loginCommandHandler.Handle(command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.AccessToken.ShouldBe("jwt-token");
        result.Value.AccountId.ShouldBe(owner.Id);
    }

    [Fact]
    public async Task Handle_When_OwnerNotFound_Returns_NotFoundError()
    {
        _ownerRepository.GetByUsernameAsync("ghost", CancellationToken.None).Returns((Owner?)null);

        var command = new LoginCommand("ghost", "pass123");

        var result = await _loginCommandHandler.Handle(command, CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeOfType<NotFoundError>();
    }

    [Fact]
    public async Task Handle_When_PasswordIsWrong_Returns_NotFoundError()
    {
        var owner = new OwnerBuilder().WithUsername("alice").Build();
        _ownerRepository.GetByUsernameAsync("alice", CancellationToken.None).Returns(owner);
        _passwordHasher.Verify("wrongpass", owner.PasswordHash).Returns(false);

        var command = new LoginCommand("alice", "wrongpass");

        var result = await _loginCommandHandler.Handle(command, CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeOfType<NotFoundError>();
    }

    [Fact]
    public async Task Handle_When_OwnerIsInactive_Returns_ForbiddenError()
    {
        var owner = new OwnerBuilder().WithUsername("inactive").Build();
        owner.SetIsActive(false);
        _ownerRepository.GetByUsernameAsync("inactive", CancellationToken.None).Returns(owner);
        _passwordHasher.Verify("pass123", owner.PasswordHash).Returns(true);

        var command = new LoginCommand("inactive", "pass123");

        var result = await _loginCommandHandler.Handle(command, CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeOfType<ForbiddenError>();
    }
}
