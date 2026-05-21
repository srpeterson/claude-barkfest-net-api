using Barkfest.Application.Features.Pets.Commands.CreatePet;
using Barkfest.Domain.Entities;

namespace Barkfest.Application.Tests.Features.Pets.Commands;

public class CreatePetCommandValidatorTests
{
    private readonly CreatePetCommandValidator _createPetCommandValidator = new();

    // -----------------------------------------------------------------------
    // Valid command
    // -----------------------------------------------------------------------

    [Fact]
    public void Validate_When_CommandIsValid_Passes()
    {
        var command = new CreatePetCommand("Bruno", null, null, "Dog", "Beagle");

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
        var command = new CreatePetCommand(name!, null, null, "Dog", "Beagle");

        var result = _createPetCommandValidator.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(command.Name));
    }

    [Fact]
    public void Validate_When_NameExceedsMaxLength_Fails_ForName()
    {
        var command = new CreatePetCommand(
            new string('A', Pet.NameMaxLength + 1), null, null, "Dog", "Beagle");

        var result = _createPetCommandValidator.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(command.Name));
    }

    // -----------------------------------------------------------------------
    // PetType
    // -----------------------------------------------------------------------

    [Theory]
    [InlineData("Dog")]
    [InlineData("Cat")]
    public void Validate_When_PetTypeIsKnown_Passes(string petType)
    {
        var breed = petType == "Dog" ? "Beagle" : "Siamese";
        var command = new CreatePetCommand("Bruno", null, null, petType, breed);

        _createPetCommandValidator.Validate(command).IsValid.ShouldBeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validate_When_PetTypeIsEmptyOrNull_Fails_ForPetType(string? petType)
    {
        var command = new CreatePetCommand("Bruno", null, null, petType!, "Beagle");

        var result = _createPetCommandValidator.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(command.PetType));
    }

    [Fact]
    public void Validate_When_PetTypeIsUnknown_Fails_ForPetType()
    {
        var command = new CreatePetCommand("Bruno", null, null, "Other", "Beagle");

        var result = _createPetCommandValidator.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(command.PetType));
    }

    // -----------------------------------------------------------------------
    // Breed
    // -----------------------------------------------------------------------

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validate_When_BreedIsEmptyOrNull_And_PetTypeIsDog_Fails_ForBreed(string? breed)
    {
        var command = new CreatePetCommand("Bruno", null, null, "Dog", breed!);

        var result = _createPetCommandValidator.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(command.Breed));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validate_When_BreedIsEmptyOrNull_And_PetTypeIsCat_Fails_ForBreed(string? breed)
    {
        var command = new CreatePetCommand("Bruno", null, null, "Cat", breed!);

        var result = _createPetCommandValidator.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(command.Breed));
    }

    [Fact]
    public void Validate_When_BreedIsInvalidDogBreed_Fails_ForBreed()
    {
        var command = new CreatePetCommand("Bruno", null, null, "Dog", "InvalidBreed");

        var result = _createPetCommandValidator.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e =>
            e.PropertyName == nameof(command.Breed) && e.ErrorMessage == "Invalid dog breed.");
    }

    [Fact]
    public void Validate_When_BreedIsInvalidCatBreed_Fails_ForBreed()
    {
        var command = new CreatePetCommand("Bruno", null, null, "Cat", "InvalidBreed");

        var result = _createPetCommandValidator.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e =>
            e.PropertyName == nameof(command.Breed) && e.ErrorMessage == "Invalid cat breed.");
    }

    [Fact]
    public void Validate_When_CatBreedProvidedForDog_Fails_ForBreed()
    {
        var command = new CreatePetCommand("Bruno", null, null, "Dog", "Siamese");

        var result = _createPetCommandValidator.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e =>
            e.PropertyName == nameof(command.Breed) && e.ErrorMessage == "Invalid dog breed.");
    }

    [Fact]
    public void Validate_When_DogBreedProvidedForCat_Fails_ForBreed()
    {
        var command = new CreatePetCommand("Bruno", null, null, "Cat", "Beagle");

        var result = _createPetCommandValidator.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e =>
            e.PropertyName == nameof(command.Breed) && e.ErrorMessage == "Invalid cat breed.");
    }
}
