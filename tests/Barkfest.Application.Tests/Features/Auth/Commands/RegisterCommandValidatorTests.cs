using Barkfest.Application.Features.Auth.Commands.Register;
using Barkfest.Domain.Entities;

namespace Barkfest.Application.Tests.Features.Auth.Commands;

public class RegisterCommandValidatorTests
{
    private readonly RegisterCommandValidator _registerCommandValidator = new();

    // -----------------------------------------------------------------------
    // Valid command
    // -----------------------------------------------------------------------

    [Fact]
    public void Validate_When_CommandIsValid_Passes()
    {
        var command = new RegisterCommand("Alice", "Adams", "alice@example.com", null, "SecurePass1!");

        _registerCommandValidator.Validate(command).IsValid.ShouldBeTrue();
    }

    // -----------------------------------------------------------------------
    // FirstName
    // -----------------------------------------------------------------------

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validate_When_FirstNameIsEmptyOrNull_Fails_ForFirstName(string? firstName)
    {
        var command = new RegisterCommand(firstName!, "Adams", "alice@example.com", null, "pass");

        var result = _registerCommandValidator.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(command.FirstName));
    }

    [Fact]
    public void Validate_When_FirstNameExceedsMaxLength_Fails_ForFirstName()
    {
        var command = new RegisterCommand(
            new string('A', Owner.FirstNameMaxLength + 1), "Adams", "alice@example.com", null, "pass");

        var result = _registerCommandValidator.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(command.FirstName));
    }

    // -----------------------------------------------------------------------
    // LastName
    // -----------------------------------------------------------------------

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validate_When_LastNameIsEmptyOrNull_Fails_ForLastName(string? lastName)
    {
        var command = new RegisterCommand("Alice", lastName!, "alice@example.com", null, "pass");

        var result = _registerCommandValidator.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(command.LastName));
    }

    [Fact]
    public void Validate_When_LastNameExceedsMaxLength_Fails_ForLastName()
    {
        var command = new RegisterCommand(
            "Alice", new string('A', Owner.LastNameMaxLength + 1), "alice@example.com", null, "pass");

        var result = _registerCommandValidator.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(command.LastName));
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
        var command = new RegisterCommand("Alice", "Adams", email!, null, "pass");

        var result = _registerCommandValidator.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(command.Email));
    }

    [Fact]
    public void Validate_When_EmailIsInvalidFormat_Fails_ForEmail()
    {
        var command = new RegisterCommand("Alice", "Adams", "not-an-email", null, "pass");

        var result = _registerCommandValidator.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(command.Email));
    }

    [Fact]
    public void Validate_When_EmailExceedsMaxLength_Fails_ForEmail()
    {
        var longEmail = new string('a', Owner.EmailMaxLength) + "@example.com";
        var command = new RegisterCommand("Alice", "Adams", longEmail, null, "pass");

        var result = _registerCommandValidator.Validate(command);

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
        var command = new RegisterCommand("Alice", "Adams", "alice@example.com", null, password!);

        var result = _registerCommandValidator.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(command.Password));
    }
}
