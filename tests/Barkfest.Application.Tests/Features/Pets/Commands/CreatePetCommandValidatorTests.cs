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
    public void Validate_When_CommandIsValid_Passes()
    {
        var command = new CreatePetCommand(Guid.NewGuid(), "Bruno", null, null, "Dog");

        _sut.Validate(command).IsValid.ShouldBeTrue();
    }

    // -----------------------------------------------------------------------
    // OwnerId
    // -----------------------------------------------------------------------

    [Fact]
    public void Validate_When_OwnerIdIsEmpty_Fails_ForOwnerId()
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
    public void Validate_When_NameIsEmptyOrNull_Fails_ForName(string? name)
    {
        var command = new CreatePetCommand(Guid.NewGuid(), name!, null, null, "Dog");

        var result = _sut.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(command.Name));
    }

    [Fact]
    public void Validate_When_NameExceedsMaxLength_Fails_ForName()
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
    public void Validate_When_PetTypeIsKnown_Passes(string petType)
    {
        var command = new CreatePetCommand(Guid.NewGuid(), "Bruno", null, null, petType);

        _sut.Validate(command).IsValid.ShouldBeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validate_When_PetTypeIsEmptyOrNull_Fails_ForPetType(string? petType)
    {
        var command = new CreatePetCommand(Guid.NewGuid(), "Bruno", null, null, petType!);

        var result = _sut.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(command.PetType));
    }
}
