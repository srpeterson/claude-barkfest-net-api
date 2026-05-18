using Barkfest.Application.Features.Owners.Commands.CreateOwner;
using Barkfest.Domain.Entities;

namespace Barkfest.Application.Tests.Features.Owners.Commands;

public class CreateOwnerCommandValidatorTests
{
    private readonly CreateOwnerCommandValidator _createOwnerCommandValidator = new();

    // -----------------------------------------------------------------------
    // Valid command
    // -----------------------------------------------------------------------

    [Fact]
    public void Validate_When_CommandIsValid_Passes()
    {
        var command = new CreateOwnerCommand("Alice", "Smith", "alice@example.com", "555-0100");

        _createOwnerCommandValidator.Validate(command).IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_When_PhoneNumberIsNull_Passes()
    {
        var command = new CreateOwnerCommand("Alice", "Smith", "alice@example.com", null);

        _createOwnerCommandValidator.Validate(command).IsValid.ShouldBeTrue();
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
        var command = new CreateOwnerCommand(firstName!, "Smith", "alice@example.com", null);

        var result = _createOwnerCommandValidator.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(command.FirstName));
    }

    [Fact]
    public void Validate_When_FirstNameExceedsMaxLength_Fails_ForFirstName()
    {
        var command = new CreateOwnerCommand(
            new string('A', Owner.FirstNameMaxLength + 1), "Smith", "alice@example.com", null);

        var result = _createOwnerCommandValidator.Validate(command);

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
        var command = new CreateOwnerCommand("Alice", lastName!, "alice@example.com", null);

        var result = _createOwnerCommandValidator.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(command.LastName));
    }

    [Fact]
    public void Validate_When_LastNameExceedsMaxLength_Fails_ForLastName()
    {
        var command = new CreateOwnerCommand(
            "Alice", new string('A', Owner.LastNameMaxLength + 1), "alice@example.com", null);

        var result = _createOwnerCommandValidator.Validate(command);

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
        var command = new CreateOwnerCommand("Alice", "Smith", email!, null);

        var result = _createOwnerCommandValidator.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(command.Email));
    }

    [Theory]
    [InlineData("notanemail")]
    [InlineData("@nodomain.com")]
    [InlineData("missing@")]
    public void Validate_When_EmailFormatIsInvalid_Fails_ForEmail(string email)
    {
        var command = new CreateOwnerCommand("Alice", "Smith", email, null);

        var result = _createOwnerCommandValidator.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(command.Email));
    }

    [Fact]
    public void Validate_When_EmailExceedsMaxLength_Fails_ForEmail()
    {
        var localPart = new string('a', Owner.EmailMaxLength - "@b.co".Length + 1);
        var command = new CreateOwnerCommand("Alice", "Smith", $"{localPart}@b.co", null);

        var result = _createOwnerCommandValidator.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(command.Email));
    }
}
