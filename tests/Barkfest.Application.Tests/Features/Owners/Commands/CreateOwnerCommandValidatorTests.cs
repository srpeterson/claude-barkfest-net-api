using Barkfest.Application.Features.Owners.Commands.CreateOwner;
using Barkfest.Domain.Entities;

namespace Barkfest.Application.Tests.Features.Owners.Commands;

public class CreateOwnerCommandValidatorTests
{
    private readonly CreateOwnerCommandValidator _sut = new();

    // -----------------------------------------------------------------------
    // Valid command
    // -----------------------------------------------------------------------

    [Fact]
    public void Validate_ValidCommand_Passes()
    {
        var command = new CreateOwnerCommand("Alice", "Smith", "alice@example.com", "555-0100");

        _sut.Validate(command).IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_NullPhoneNumber_Passes()
    {
        var command = new CreateOwnerCommand("Alice", "Smith", "alice@example.com", null);

        _sut.Validate(command).IsValid.ShouldBeTrue();
    }

    // -----------------------------------------------------------------------
    // FirstName
    // -----------------------------------------------------------------------

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validate_FirstNameEmptyOrNull_FailsOnFirstName(string? firstName)
    {
        var command = new CreateOwnerCommand(firstName!, "Smith", "alice@example.com", null);

        var result = _sut.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(command.FirstName));
    }

    [Fact]
    public void Validate_FirstNameAtMaxLength_Passes()
    {
        var command = new CreateOwnerCommand(
            new string('A', Owner.FirstNameMaxLength), "Smith", "alice@example.com", null);

        _sut.Validate(command).IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_FirstNameExceedsMaxLength_FailsOnFirstName()
    {
        var command = new CreateOwnerCommand(
            new string('A', Owner.FirstNameMaxLength + 1), "Smith", "alice@example.com", null);

        var result = _sut.Validate(command);

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
    public void Validate_LastNameEmptyOrNull_FailsOnLastName(string? lastName)
    {
        var command = new CreateOwnerCommand("Alice", lastName!, "alice@example.com", null);

        var result = _sut.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(command.LastName));
    }

    [Fact]
    public void Validate_LastNameAtMaxLength_Passes()
    {
        var command = new CreateOwnerCommand(
            "Alice", new string('A', Owner.LastNameMaxLength), "alice@example.com", null);

        _sut.Validate(command).IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_LastNameExceedsMaxLength_FailsOnLastName()
    {
        var command = new CreateOwnerCommand(
            "Alice", new string('A', Owner.LastNameMaxLength + 1), "alice@example.com", null);

        var result = _sut.Validate(command);

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
    public void Validate_EmailEmptyOrNull_FailsOnEmail(string? email)
    {
        var command = new CreateOwnerCommand("Alice", "Smith", email!, null);

        var result = _sut.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(command.Email));
    }

    [Theory]
    [InlineData("notanemail")]
    [InlineData("@nodomain.com")]
    [InlineData("missing@")]
    public void Validate_EmailInvalidFormat_FailsOnEmail(string email)
    {
        var command = new CreateOwnerCommand("Alice", "Smith", email, null);

        var result = _sut.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(command.Email));
    }

    [Fact]
    public void Validate_EmailAtMaxLength_Passes()
    {
        var localPart = new string('a', Owner.EmailMaxLength - "@b.co".Length);
        var command = new CreateOwnerCommand("Alice", "Smith", $"{localPart}@b.co", null);

        _sut.Validate(command).IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_EmailExceedsMaxLength_FailsOnEmail()
    {
        var localPart = new string('a', Owner.EmailMaxLength - "@b.co".Length + 1);
        var command = new CreateOwnerCommand("Alice", "Smith", $"{localPart}@b.co", null);

        var result = _sut.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(command.Email));
    }
}
