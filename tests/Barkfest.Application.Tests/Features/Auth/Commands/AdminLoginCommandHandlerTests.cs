using Barkfest.Application.Common.Exceptions;
using Barkfest.Application.Common.Interfaces;
using Barkfest.Application.Features.Auth.Commands.AdminLogin;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Interfaces;
using NSubstitute;

namespace Barkfest.Application.Tests.Features.Auth.Commands;

public class AdminLoginCommandHandlerTests
{
    private readonly IAdministratorRepository _administratorRepository = Substitute.For<IAdministratorRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly IJwtTokenService _jwtTokenService = Substitute.For<IJwtTokenService>();
    private readonly AdminLoginCommandHandler _adminLoginCommandHandler;

    public AdminLoginCommandHandlerTests()
    {
        _adminLoginCommandHandler = new AdminLoginCommandHandler(_administratorRepository, _passwordHasher, _jwtTokenService);
    }

    [Fact]
    public async Task Handle_When_CredentialsAreValid_Returns_AuthTokenDto()
    {
        var administrator = new Administrator();
        administrator.SetUsername("admin");
        administrator.SetEmail("admin@barkfest.dev");
        administrator.SetPasswordHash("$2a$11$hash");
        _administratorRepository.GetByUsernameAsync("admin", CancellationToken.None).Returns(administrator);
        _passwordHasher.Verify("secretpass", administrator.PasswordHash).Returns(true);
        _jwtTokenService.GenerateAdminToken(administrator).Returns("admin-jwt-token");
        _jwtTokenService.GetExpiry().Returns(DateTime.UtcNow.AddHours(1));

        var result = await _adminLoginCommandHandler.Handle(
            new AdminLoginCommand("admin", "secretpass"), CancellationToken.None);

        result.AccessToken.ShouldBe("admin-jwt-token");
        result.AccountId.ShouldBe(administrator.Id);
    }

    [Fact]
    public async Task Handle_When_UsernameNotFound_Throws_NotFoundException()
    {
        _administratorRepository.GetByUsernameAsync("ghost", CancellationToken.None).Returns((Administrator?)null);

        await Should.ThrowAsync<NotFoundException>(
            () => _adminLoginCommandHandler.Handle(
                new AdminLoginCommand("ghost", "pass"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_When_PasswordIsWrong_Throws_NotFoundException()
    {
        var administrator = new Administrator();
        administrator.SetUsername("admin");
        administrator.SetEmail("admin@barkfest.dev");
        administrator.SetPasswordHash("$2a$11$hash");
        _administratorRepository.GetByUsernameAsync("admin", CancellationToken.None).Returns(administrator);
        _passwordHasher.Verify("wrongpass", administrator.PasswordHash).Returns(false);

        await Should.ThrowAsync<NotFoundException>(
            () => _adminLoginCommandHandler.Handle(
                new AdminLoginCommand("admin", "wrongpass"), CancellationToken.None));
    }
}
