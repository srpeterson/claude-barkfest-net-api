using Barkfest.Application.Features.Pets.Commands.CreatePet;
using Barkfest.Domain.Entities;

namespace Barkfest.Application.Tests.Features.Pets.Commands;

public class CreatePetCommandValidatorTests
{
    private readonly CreatePetCommandValidator _sut = new();

    // -----------------------------------------------------------------------
    // Valid command
    // -----------------------------------------------------------------------

    [Fact]
    public void Validate_ValidCommand_Passes()
    {
        var command = new CreatePetCommand(Guid.NewGuid(), "Bruno", null, null, "Dog");

        _sut.Validate(command).IsValid.ShouldBeTrue();
    }

    // -----------------------------------------------------------------------
    // OwnerId
    // -----------------------------------------------------------------------

    [Fact]
    public void Validate_EmptyOwnerId_FailsOnOwnerId()
    {
        var command = new CreatePetCommand(Guid.Empty, "Bruno", null, null, "Dog");

        var result = _sut.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(command.OwnerId));
    }

    // -----------------------------------------------------------------------
    // Name
    // -----------------------------------------------------------------------

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validate_NameEmptyOrNull_FailsOnName(string? name)
    {
        var command = new CreatePetCommand(Guid.NewGuid(), name!, null, null, "Dog");

        var result = _sut.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(command.Name));
    }

    [Fact]
    public void Validate_NameAtMaxLength_Passes()
    {
        var command = new CreatePetCommand(
            Guid.NewGuid(), new string('A', Pet.NameMaxLength), null, null, "Dog");

        _sut.Validate(command).IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_NameExceedsMaxLength_FailsOnName()
    {
        var command = new CreatePetCommand(
            Guid.NewGuid(), new string('A', Pet.NameMaxLength + 1), null, null, "Dog");

        var result = _sut.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(command.Name));
    }

    // -----------------------------------------------------------------------
    // PetType
    // -----------------------------------------------------------------------

    [Theory]
    [InlineData("Dog")]
    [InlineData("Cat")]
    [InlineData("Other")]
    public void Validate_KnownPetType_Passes(string petType)
    {
        var command = new CreatePetCommand(Guid.NewGuid(), "Bruno", null, null, petType);

        _sut.Validate(command).IsValid.ShouldBeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validate_PetTypeEmptyOrNull_FailsOnPetType(string? petType)
    {
        var command = new CreatePetCommand(Guid.NewGuid(), "Bruno", null, null, petType!);

        var result = _sut.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(command.PetType));
    }
}
