using Barkfest.Application.Features.Pets.Commands.UpdatePet;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Enums;

namespace Barkfest.Application.Tests.Features.Pets.Commands;

public class UpdatePetCommandValidatorTests
{
    private readonly UpdatePetCommandValidator _updatePetCommandValidator = new();

    // -----------------------------------------------------------------------
    // Valid command
    // -----------------------------------------------------------------------

    [Fact]
    public void Validate_When_CommandIsValid_Dog_Passes()
    {
        var command = new UpdatePetCommand(Guid.NewGuid(), "Bruno", null, null, PetType.Dog.Value, DogBreed.Beagle.Value);

        _updatePetCommandValidator.Validate(command).IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_When_CommandIsValid_Cat_Passes()
    {
        var command = new UpdatePetCommand(Guid.NewGuid(), "Whiskers", null, null, PetType.Cat.Value, CatBreed.Siamese.Value);

        _updatePetCommandValidator.Validate(command).IsValid.ShouldBeTrue();
    }

    // -----------------------------------------------------------------------
    // Id
    // -----------------------------------------------------------------------

    [Fact]
    public void Validate_When_PetIdIsEmpty_Fails_ForPetId()
    {
        var command = new UpdatePetCommand(Guid.Empty, "Bruno", null, null, PetType.Dog.Value, DogBreed.Beagle.Value);

        var result = _updatePetCommandValidator.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(command.PetId));
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
        var command = new UpdatePetCommand(Guid.NewGuid(), name!, null, null, PetType.Dog.Value, DogBreed.Beagle.Value);

        var result = _updatePetCommandValidator.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(command.Name));
    }

    [Fact]
    public void Validate_When_NameExceedsMaxLength_Fails_ForName()
    {
        var command = new UpdatePetCommand(
            Guid.NewGuid(), new string('A', Pet.NameMaxLength + 1), null, null, PetType.Dog.Value, DogBreed.Beagle.Value);

        var result = _updatePetCommandValidator.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(command.Name));
    }

    // -----------------------------------------------------------------------
    // PetTypeValue
    // -----------------------------------------------------------------------

    [Theory]
    [InlineData(1)] // Dog
    [InlineData(2)] // Cat
    public void Validate_When_PetTypeValueIsValid_Passes(int petTypeValue)
    {
        var breedValue = petTypeValue == PetType.Dog.Value ? DogBreed.Beagle.Value : CatBreed.Siamese.Value;
        var command = new UpdatePetCommand(Guid.NewGuid(), "Bruno", null, null, petTypeValue, breedValue);

        _updatePetCommandValidator.Validate(command).IsValid.ShouldBeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(99)]
    [InlineData(-1)]
    public void Validate_When_PetTypeValueIsInvalid_Fails_ForPetTypeValue(int petTypeValue)
    {
        var command = new UpdatePetCommand(Guid.NewGuid(), "Bruno", null, null, petTypeValue, DogBreed.Beagle.Value);

        var result = _updatePetCommandValidator.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(command.PetTypeValue));
    }

    // -----------------------------------------------------------------------
    // BreedValue
    // -----------------------------------------------------------------------

    [Fact]
    public void Validate_When_BreedIsValidDogBreed_ForDog_Passes()
    {
        var command = new UpdatePetCommand(Guid.NewGuid(), "Bruno", null, null, PetType.Dog.Value, DogBreed.GoldenRetriever.Value);

        _updatePetCommandValidator.Validate(command).IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_When_BreedIsValidCatBreed_ForCat_Passes()
    {
        var command = new UpdatePetCommand(Guid.NewGuid(), "Whiskers", null, null, PetType.Cat.Value, CatBreed.MaineCoon.Value);

        _updatePetCommandValidator.Validate(command).IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_When_DogExclusiveBreedValueUsedForCat_Fails_ForBreedValue()
    {
        // DogBreed.Other = 30; CatBreed.Other = 29 — value 30 is not a valid CatBreed value
        var command = new UpdatePetCommand(Guid.NewGuid(), "Whiskers", null, null, PetType.Cat.Value, DogBreed.Other.Value);

        var result = _updatePetCommandValidator.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(command.BreedValue) && e.ErrorMessage == "Invalid breed.");
    }

    [Fact]
    public void Validate_When_BreedValueIsUnrecognised_Fails_ForBreedValue()
    {
        var command = new UpdatePetCommand(Guid.NewGuid(), "Bruno", null, null, PetType.Dog.Value, 9999);

        var result = _updatePetCommandValidator.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(command.BreedValue));
    }

    [Fact]
    public void Validate_When_PetTypeValueIsInvalid_DoesNotReport_BreedValueError()
    {
        var command = new UpdatePetCommand(Guid.NewGuid(), "Bruno", null, null, 99, 9999);

        var result = _updatePetCommandValidator.Validate(command);

        result.Errors.ShouldNotContain(e => e.PropertyName == nameof(command.BreedValue));
    }
}
