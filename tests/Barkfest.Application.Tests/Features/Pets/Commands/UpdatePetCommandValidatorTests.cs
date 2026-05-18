using Barkfest.Application.Features.Pets.Commands.UpdatePet;
using Barkfest.Domain.Entities;

namespace Barkfest.Application.Tests.Features.Pets.Commands;

public class UpdatePetCommandValidatorTests
{
    private readonly UpdatePetCommandValidator _updatePetCommandValidator = new();

    // -----------------------------------------------------------------------
    // Valid command
    // -----------------------------------------------------------------------

    [Fact]
    public void Validate_When_CommandIsValid_Passes()
    {
        var command = new UpdatePetCommand(Guid.NewGuid(), "Bruno", null, null, "Dog");

        _updatePetCommandValidator.Validate(command).IsValid.ShouldBeTrue();
    }

    // -----------------------------------------------------------------------
    // Id
    // -----------------------------------------------------------------------

    [Fact]
    public void Validate_When_IdIsEmpty_Fails_ForId()
    {
        var command = new UpdatePetCommand(Guid.Empty, "Bruno", null, null, "Dog");

        var result = _updatePetCommandValidator.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(command.Id));
    }

    // -----------------------------------------------------------------------
    // Name
    // -----------------------------------------------------------------------

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validate_When_NameIsEmptyOrNull_Fails_ForName(string? name)
    {
        var command = new UpdatePetCommand(Guid.NewGuid(), name!, null, null, "Dog");

        var result = _updatePetCommandValidator.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(command.Name));
    }

    [Fact]
    public void Validate_When_NameExceedsMaxLength_Fails_ForName()
    {
        var command = new UpdatePetCommand(
            Guid.NewGuid(), new string('A', Pet.NameMaxLength + 1), null, null, "Dog");

        var result = _updatePetCommandValidator.Validate(command);

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
    public void Validate_When_PetTypeIsKnown_Passes(string petType)
    {
        var command = new UpdatePetCommand(Guid.NewGuid(), "Bruno", null, null, petType);

        _updatePetCommandValidator.Validate(command).IsValid.ShouldBeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validate_When_PetTypeIsEmptyOrNull_Fails_ForPetType(string? petType)
    {
        var command = new UpdatePetCommand(Guid.NewGuid(), "Bruno", null, null, petType!);

        var result = _updatePetCommandValidator.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(command.PetType));
    }
}
