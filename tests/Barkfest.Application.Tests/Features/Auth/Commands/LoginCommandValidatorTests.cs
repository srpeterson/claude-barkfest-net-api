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
        var command = new LoginCommand("alice", "pass123");

        _loginCommandValidator.Validate(command).IsValid.ShouldBeTrue();
    }

    // -----------------------------------------------------------------------
    // Username
    // -----------------------------------------------------------------------

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validate_When_UsernameIsEmptyOrNull_Fails_ForUsername(string? username)
    {
        var command = new LoginCommand(username!, "pass123");

        var result = _loginCommandValidator.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(command.Username));
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
        var command = new LoginCommand("alice", password!);

        var result = _loginCommandValidator.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(command.Password));
    }
}
