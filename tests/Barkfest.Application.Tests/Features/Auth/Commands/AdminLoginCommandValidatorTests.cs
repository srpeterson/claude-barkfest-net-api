using Barkfest.Application.Features.Auth.Commands.AdminLogin;

namespace Barkfest.Application.Tests.Features.Auth.Commands;

public class AdminLoginCommandValidatorTests
{
    private readonly AdminLoginCommandValidator _adminLoginCommandValidator = new();

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Fails_ForEmail_When_Empty(string email)
    {
        var result = _adminLoginCommandValidator.Validate(new AdminLoginCommand(email, "pass"));

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(AdminLoginCommand.Email));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Fails_ForPassword_When_Empty(string password)
    {
        var result = _adminLoginCommandValidator.Validate(new AdminLoginCommand("admin@barkfest.dev", password));

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(AdminLoginCommand.Password));
    }
}
