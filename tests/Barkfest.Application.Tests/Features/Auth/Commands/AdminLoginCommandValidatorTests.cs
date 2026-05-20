using Barkfest.Application.Features.Auth.Commands.AdminLogin;

namespace Barkfest.Application.Tests.Features.Auth.Commands;

public class AdminLoginCommandValidatorTests
{
    private readonly AdminLoginCommandValidator _adminLoginCommandValidator = new();

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Fails_ForUsername_When_Empty(string username)
    {
        var result = _adminLoginCommandValidator.Validate(new AdminLoginCommand(username, "pass"));

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(AdminLoginCommand.Username));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Fails_ForPassword_When_Empty(string password)
    {
        var result = _adminLoginCommandValidator.Validate(new AdminLoginCommand("admin", password));

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(AdminLoginCommand.Password));
    }
}
