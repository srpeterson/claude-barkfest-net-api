using Barkfest.Application.Features.Auth.Commands.Login;

namespace Barkfest.Application.Tests.Features.Auth.Commands;

public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _loginCommandValidator = new();

    // -----------------------------------------------------------------------
    // Valid command
    // -----------------------------------------------------------------------

    [Fact]
    public void Validate_When_CommandIsValid_Passes()
    {
        var command = new LoginCommand("alice@example.com", "pass123");

        _loginCommandValidator.Validate(command).IsValid.ShouldBeTrue();
    }

    // -----------------------------------------------------------------------
    // Email
    // -----------------------------------------------------------------------

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validate_When_EmailIsEmptyOrNull_Fails_ForEmail(string? email)
    {
        var command = new LoginCommand(email!, "pass123");

        var result = _loginCommandValidator.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(command.Email));
    }

    // -----------------------------------------------------------------------
    // Password
    // -----------------------------------------------------------------------

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validate_When_PasswordIsEmptyOrNull_Fails_ForPassword(string? password)
    {
        var command = new LoginCommand("alice@example.com", password!);

        var result = _loginCommandValidator.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(command.Password));
    }
}
