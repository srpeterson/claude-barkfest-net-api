using Barkfest.Application.Features.Pets.Commands.CreatePet;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Enums;

namespace Barkfest.Application.Tests.Features.Pets.Commands;

public class CreatePetCommandValidatorTests
{
    private readonly CreatePetCommandValidator _createPetCommandValidator = new();

    // -----------------------------------------------------------------------
    // Valid command
    // -----------------------------------------------------------------------

    [Fact]
    public void Validate_When_CommandIsValid_Dog_Passes()
    {
        var command = new CreatePetCommand("Bruno", null, null, PetType.Dog.Value, DogBreed.Beagle.Value);

        _createPetCommandValidator.Validate(command).IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_When_CommandIsValid_Cat_Passes()
    {
        var command = new CreatePetCommand("Whiskers", null, null, PetType.Cat.Value, CatBreed.Siamese.Value);

        _createPetCommandValidator.Validate(command).IsValid.ShouldBeTrue();
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
        var command = new CreatePetCommand(name!, null, null, PetType.Dog.Value, DogBreed.Beagle.Value);

        var result = _createPetCommandValidator.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(command.Name));
    }

    [Fact]
    public void Validate_When_NameExceedsMaxLength_Fails_ForName()
    {
        var command = new CreatePetCommand(
            new string('A', Pet.NameMaxLength + 1), null, null, PetType.Dog.Value, DogBreed.Beagle.Value);

        var result = _createPetCommandValidator.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(command.Name));
    }

    // -----------------------------------------------------------------------
    // PetTypeValue
    // -----------------------------------------------------------------------

    [Theory]
    [InlineData(0)]
    [InlineData(99)]
    [InlineData(-1)]
    public void Validate_When_PetTypeValueIsInvalid_Fails_ForPetTypeValue(int petTypeValue)
    {
        var command = new CreatePetCommand("Bruno", null, null, petTypeValue, DogBreed.Beagle.Value);

        var result = _createPetCommandValidator.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(command.PetTypeValue));
    }

    // -----------------------------------------------------------------------
    // BreedValue
    // -----------------------------------------------------------------------

    [Fact]
    public void Validate_When_BreedIsValidDogBreed_ForDog_Passes()
    {
        var command = new CreatePetCommand("Bruno", null, null, PetType.Dog.Value, DogBreed.GoldenRetriever.Value);

        _createPetCommandValidator.Validate(command).IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_When_BreedIsValidCatBreed_ForCat_Passes()
    {
        var command = new CreatePetCommand("Whiskers", null, null, PetType.Cat.Value, CatBreed.MaineCoon.Value);

        _createPetCommandValidator.Validate(command).IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_When_DogExclusiveBreedValueUsedForCat_Fails_ForBreedValue()
    {
        // DogBreed.Other = 30; CatBreed.Other = 29 — value 30 is not a valid CatBreed value
        var command = new CreatePetCommand("Whiskers", null, null, PetType.Cat.Value, DogBreed.Other.Value);

        var result = _createPetCommandValidator.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(command.BreedValue) && e.ErrorMessage == "Invalid breed.");
    }

    [Fact]
    public void Validate_When_BreedValueIsUnrecognised_Fails_ForBreedValue()
    {
        var command = new CreatePetCommand("Bruno", null, null, PetType.Dog.Value, 9999);

        var result = _createPetCommandValidator.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(command.BreedValue));
    }

    [Fact]
    public void Validate_When_PetTypeValueIsInvalid_DoesNotReport_BreedValueError()
    {
        var command = new CreatePetCommand("Bruno", null, null, 99, 9999);

        var result = _createPetCommandValidator.Validate(command);

        result.Errors.ShouldNotContain(e => e.PropertyName == nameof(command.BreedValue));
    }
}
