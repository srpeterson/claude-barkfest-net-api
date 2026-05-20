using Barkfest.Application.Common.Exceptions;
using Barkfest.Application.Common.Interfaces;
using Barkfest.Application.Features.Auth.Commands.Login;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Exceptions;
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

        result.AccessToken.ShouldBe("jwt-token");
        result.AccountId.ShouldBe(owner.Id);
    }

    [Fact]
    public async Task Handle_When_OwnerNotFound_Throws_NotFoundException()
    {
        _ownerRepository.GetByUsernameAsync("ghost", CancellationToken.None).Returns((Owner?)null);

        var command = new LoginCommand("ghost", "pass123");

        await Should.ThrowAsync<NotFoundException>(() => _loginCommandHandler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_When_PasswordIsWrong_Throws_NotFoundException()
    {
        var owner = new OwnerBuilder().WithUsername("alice").Build();
        _ownerRepository.GetByUsernameAsync("alice", CancellationToken.None).Returns(owner);
        _passwordHasher.Verify("wrongpass", owner.PasswordHash).Returns(false);

        var command = new LoginCommand("alice", "wrongpass");

        await Should.ThrowAsync<NotFoundException>(() => _loginCommandHandler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_When_OwnerIsInactive_Throws_ForbiddenException()
    {
        var owner = new OwnerBuilder().WithUsername("inactive").Build();
        owner.SetActive(false);
        _ownerRepository.GetByUsernameAsync("inactive", CancellationToken.None).Returns(owner);
        _passwordHasher.Verify("pass123", owner.PasswordHash).Returns(true);

        var command = new LoginCommand("inactive", "pass123");

        await Should.ThrowAsync<ForbiddenException>(() => _loginCommandHandler.Handle(command, CancellationToken.None));
    }
}
