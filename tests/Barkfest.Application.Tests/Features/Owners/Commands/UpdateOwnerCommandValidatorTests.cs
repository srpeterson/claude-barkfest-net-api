using Barkfest.Application.Features.Owners.Commands.UpdateOwner;
using Barkfest.Domain.Entities;

namespace Barkfest.Application.Tests.Features.Owners.Commands;

public class UpdateOwnerCommandValidatorTests
{
    private readonly UpdateOwnerCommandValidator _sut = new();

    // -----------------------------------------------------------------------
    // Valid command
    // -----------------------------------------------------------------------

    [Fact]
    public void Validate_When_CommandIsValid_Passes()
    {
        var command = new UpdateOwnerCommand(Guid.NewGuid(), "Alice", "Smith", "alice@example.com", null);

        _sut.Validate(command).IsValid.ShouldBeTrue();
    }

    // -----------------------------------------------------------------------
    // Id
    // -----------------------------------------------------------------------

    [Fact]
    public void Validate_When_IdIsEmpty_Fails_ForId()
    {
        var command = new UpdateOwnerCommand(Guid.Empty, "Alice", "Smith", "alice@example.com", null);

        var result = _sut.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(command.Id));
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
        var command = new UpdateOwnerCommand(Guid.NewGuid(), firstName!, "Smith", "alice@example.com", null);

        var result = _sut.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(command.FirstName));
    }

    [Fact]
    public void Validate_When_FirstNameExceedsMaxLength_Fails_ForFirstName()
    {
        var command = new UpdateOwnerCommand(
            Guid.NewGuid(), new string('A', Owner.FirstNameMaxLength + 1), "Smith", "alice@example.com", null);

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
    public void Validate_When_LastNameIsEmptyOrNull_Fails_ForLastName(string? lastName)
    {
        var command = new UpdateOwnerCommand(Guid.NewGuid(), "Alice", lastName!, "alice@example.com", null);

        var result = _sut.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(command.LastName));
    }

    [Fact]
    public void Validate_When_LastNameExceedsMaxLength_Fails_ForLastName()
    {
        var command = new UpdateOwnerCommand(
            Guid.NewGuid(), "Alice", new string('A', Owner.LastNameMaxLength + 1), "alice@example.com", null);

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
    public void Validate_When_EmailIsEmptyOrNull_Fails_ForEmail(string? email)
    {
        var command = new UpdateOwnerCommand(Guid.NewGuid(), "Alice", "Smith", email!, null);

        var result = _sut.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(command.Email));
    }

    [Theory]
    [InlineData("notanemail")]
    [InlineData("@nodomain.com")]
    [InlineData("missing@")]
    public void Validate_When_EmailFormatIsInvalid_Fails_ForEmail(string email)
    {
        var command = new UpdateOwnerCommand(Guid.NewGuid(), "Alice", "Smith", email, null);

        var result = _sut.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(command.Email));
    }

    [Fact]
    public void Validate_When_EmailExceedsMaxLength_Fails_ForEmail()
    {
        var localPart = new string('a', Owner.EmailMaxLength - "@b.co".Length + 1);
        var command = new UpdateOwnerCommand(Guid.NewGuid(), "Alice", "Smith", $"{localPart}@b.co", null);

        var result = _sut.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(command.Email));
    }
}
