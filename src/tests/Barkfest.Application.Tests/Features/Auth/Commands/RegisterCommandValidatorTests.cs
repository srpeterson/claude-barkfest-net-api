using Barkfest.Application.Features.Auth.Commands.Register;
using Barkfest.Domain.Entities;
using Barkfest.Domain.ValueObjects;

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
        var command = new RegisterCommand("aliceadams", "Alice", "Adams", "alice@example.com", null, "SecurePass1!");

        _registerCommandValidator.Validate(command).IsValid.ShouldBeTrue();
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
        var command = new RegisterCommand(username!, "Alice", "Adams", "alice@example.com", null, "pass");

        var result = _registerCommandValidator.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(command.Username));
    }

    [Fact]
    public void Validate_When_UsernameExceedsMaxLength_Fails_ForUsername()
    {
        var command = new RegisterCommand(
            new string('a', AccountConstraints.UsernameMaxLength + 1), "Alice", "Adams", "alice@example.com", null, "pass");

        var result = _registerCommandValidator.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(command.Username));
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
        var command = new RegisterCommand("aliceadams", firstName!, "Adams", "alice@example.com", null, "pass");

        var result = _registerCommandValidator.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(command.FirstName));
    }

    [Fact]
    public void Validate_When_FirstNameExceedsMaxLength_Fails_ForFirstName()
    {
        var command = new RegisterCommand(
            "aliceadams", new string('A', Owner.FirstNameMaxLength + 1), "Adams", "alice@example.com", null, "pass");

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
        var command = new RegisterCommand("aliceadams", "Alice", lastName!, "alice@example.com", null, "pass");

        var result = _registerCommandValidator.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(command.LastName));
    }

    [Fact]
    public void Validate_When_LastNameExceedsMaxLength_Fails_ForLastName()
    {
        var command = new RegisterCommand(
            "aliceadams", "Alice", new string('A', Owner.LastNameMaxLength + 1), "alice@example.com", null, "pass");

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
        var command = new RegisterCommand("aliceadams", "Alice", "Adams", email!, null, "pass");

        var result = _registerCommandValidator.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(command.Email));
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("space in@example.com")]
    public void Validate_When_EmailIsInvalidFormat_Fails_ForEmail(string email)
    {
        var command = new RegisterCommand("aliceadams", "Alice", "Adams", email, null, "pass");

        var result = _registerCommandValidator.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(command.Email));
    }

    [Fact]
    public void Validate_When_EmailExceedsMaxLength_Fails_ForEmail()
    {
        var longEmail = new string('a', AccountConstraints.EmailMaxLength) + "@example.com";
        var command = new RegisterCommand("aliceadams", "Alice", "Adams", longEmail, null, "pass");

        var result = _registerCommandValidator.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(command.Email));
    }

    // -----------------------------------------------------------------------
    // PhoneNumber
    // -----------------------------------------------------------------------

    [Theory]
    [InlineData("+1-555-555-0100")]
    [InlineData("5555550100")]
    [InlineData("+0123456789")]
    [InlineData("(555) 555-0100")]
    public void Fails_ForPhoneNumber_When_NotE164Format(string phoneNumber)
    {
        var command = new RegisterCommand("aliceadams", "Alice", "Adams", "alice@example.com", phoneNumber, "pass");

        var result = _registerCommandValidator.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(command.PhoneNumber));
    }

    [Fact]
    public void Validate_When_PhoneNumberExceedsMaxLength_Fails_ForPhoneNumber()
    {
        var longPhone = new string('1', E164PhoneNumber.MaxLength + 1);
        var command = new RegisterCommand("aliceadams", "Alice", "Adams", "alice@example.com", longPhone, "SecurePass1!");

        var result = _registerCommandValidator.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(command.PhoneNumber));
    }

    [Fact]
    public void Validate_When_PhoneNumberIsNull_Passes()
    {
        var command = new RegisterCommand("aliceadams", "Alice", "Adams", "alice@example.com", null, "SecurePass1!");

        _registerCommandValidator.Validate(command).IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_When_PhoneNumberIsValidE164_Passes()
    {
        var command = new RegisterCommand("aliceadams", "Alice", "Adams", "alice@example.com", "+15555550100", "SecurePass1!");

        _registerCommandValidator.Validate(command).IsValid.ShouldBeTrue();
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
        var command = new RegisterCommand("aliceadams", "Alice", "Adams", "alice@example.com", null, password!);

        var result = _registerCommandValidator.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(command.Password));
    }

    [Fact]
    public void Validate_When_PasswordIsBelowMinLength_Fails_ForPassword()
    {
        var command = new RegisterCommand(
            "aliceadams", "Alice", "Adams", "alice@example.com", null,
            new string('a', AccountConstraints.PasswordMinLength - 1));

        var result = _registerCommandValidator.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(command.Password));
    }

    [Fact]
    public void Validate_When_PasswordExceedsMaxLength_Fails_ForPassword()
    {
        var command = new RegisterCommand(
            "aliceadams", "Alice", "Adams", "alice@example.com", null,
            new string('a', AccountConstraints.PasswordMaxLength + 1));

        var result = _registerCommandValidator.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(command.Password));
    }

    // -----------------------------------------------------------------------
    // DisplayName
    // -----------------------------------------------------------------------

    [Fact]
    public void Fails_ForDisplayName_When_ExceedsMaxLength()
    {
        var command = new RegisterCommand(
            "aliceadams", "Alice", "Adams", "alice@example.com", null, "SecurePass1!",
            new string('x', Owner.DisplayNameMaxLength + 1));

        var result = _registerCommandValidator.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(command.DisplayName));
    }

    [Fact]
    public void Validate_When_DisplayNameIsNull_Passes()
    {
        var command = new RegisterCommand("aliceadams", "Alice", "Adams", "alice@example.com", null, "SecurePass1!");

        _registerCommandValidator.Validate(command).IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_When_DisplayNameIsAtMaxLength_Passes()
    {
        var command = new RegisterCommand(
            "aliceadams", "Alice", "Adams", "alice@example.com", null, "SecurePass1!",
            new string('x', Owner.DisplayNameMaxLength));

        _registerCommandValidator.Validate(command).IsValid.ShouldBeTrue();
    }
}
