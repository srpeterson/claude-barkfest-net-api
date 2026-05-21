using Barkfest.Application.Features.Administrators.Commands.CreateAdministrator;
using Barkfest.Domain.Entities;
using Barkfest.Domain.ValueObjects;

namespace Barkfest.Application.Tests.Features.Administrators.Commands;

public class CreateAdministratorCommandValidatorTests
{
    private readonly CreateAdministratorCommandValidator _createAdministratorCommandValidator = new();

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Fails_ForUsername_When_Empty(string username)
    {
        var result = _createAdministratorCommandValidator.Validate(
            new CreateAdministratorCommand(username, "New Admin", "new@barkfest.dev", "+15555550100", "pass"));

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateAdministratorCommand.Username));
    }

    [Fact]
    public void Fails_ForUsername_When_ExceedsMaxLength()
    {
        var longUsername = new string('a', AccountConstraints.UsernameMaxLength + 1);

        var result = _createAdministratorCommandValidator.Validate(
            new CreateAdministratorCommand(longUsername, "New Admin", "new@barkfest.dev", "+15555550100", "pass"));

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateAdministratorCommand.Username));
    }

    // -----------------------------------------------------------------------
    // Name
    // -----------------------------------------------------------------------

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Fails_ForName_When_Empty(string name)
    {
        var result = _createAdministratorCommandValidator.Validate(
            new CreateAdministratorCommand("newadmin", name, "new@barkfest.dev", "+15555550100", "pass"));

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateAdministratorCommand.Name));
    }

    [Fact]
    public void Fails_ForName_When_ExceedsMaxLength()
    {
        var longName = new string('a', Administrator.NameMaxLength + 1);

        var result = _createAdministratorCommandValidator.Validate(
            new CreateAdministratorCommand("newadmin", longName, "new@barkfest.dev", "+15555550100", "pass"));

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateAdministratorCommand.Name));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Fails_ForEmail_When_Empty(string email)
    {
        var result = _createAdministratorCommandValidator.Validate(
            new CreateAdministratorCommand("newadmin", "New Admin", email, "+15555550100", "pass"));

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateAdministratorCommand.Email));
    }

    [Fact]
    public void Fails_ForEmail_When_ExceedsMaxLength()
    {
        var longEmail = new string('a', AccountConstraints.EmailMaxLength) + "@b.com";

        var result = _createAdministratorCommandValidator.Validate(
            new CreateAdministratorCommand("newadmin", "New Admin", longEmail, "+15555550100", "pass"));

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateAdministratorCommand.Email));
    }

    // -----------------------------------------------------------------------
    // PhoneNumber
    // -----------------------------------------------------------------------

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Fails_ForPhoneNumber_When_Empty(string phoneNumber)
    {
        var result = _createAdministratorCommandValidator.Validate(
            new CreateAdministratorCommand("newadmin", "New Admin", "new@barkfest.dev", phoneNumber, "pass"));

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateAdministratorCommand.PhoneNumber));
    }

    [Theory]
    [InlineData("5555555555")]
    [InlineData("(555) 555-5555")]
    [InlineData("555-555-5555")]
    [InlineData("+0123456789")]
    public void Fails_ForPhoneNumber_When_NotE164Format(string phoneNumber)
    {
        var result = _createAdministratorCommandValidator.Validate(
            new CreateAdministratorCommand("newadmin", "New Admin", "new@barkfest.dev", phoneNumber, "pass"));

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateAdministratorCommand.PhoneNumber));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Fails_ForPassword_When_Empty(string password)
    {
        var result = _createAdministratorCommandValidator.Validate(
            new CreateAdministratorCommand("newadmin", "New Admin", "new@barkfest.dev", "+15555550100", password));

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateAdministratorCommand.Password));
    }

    [Fact]
    public void Fails_ForPassword_When_BelowMinLength()
    {
        var shortPassword = new string('a', AccountConstraints.PasswordMinLength - 1);

        var result = _createAdministratorCommandValidator.Validate(
            new CreateAdministratorCommand("newadmin", "New Admin", "new@barkfest.dev", "+15555550100", shortPassword));

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateAdministratorCommand.Password));
    }

    [Fact]
    public void Fails_ForPassword_When_ExceedsMaxLength()
    {
        var longPassword = new string('a', AccountConstraints.PasswordMaxLength + 1);

        var result = _createAdministratorCommandValidator.Validate(
            new CreateAdministratorCommand("newadmin", "New Admin", "new@barkfest.dev", "+15555550100", longPassword));

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateAdministratorCommand.Password));
    }
}
