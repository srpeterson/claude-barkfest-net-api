using Barkfest.Domain.Enums;
using Shouldly;

namespace Barkfest.Domain.Tests;

public class BreedTests
{
    // -----------------------------------------------------------------------
    // IsValid
    // -----------------------------------------------------------------------

    [Fact]
    public void IsValid_When_DogBreedValueAndTypeIsDog_Returns_True()
    {
        Breed.IsValid(PetType.Dog, DogBreed.Beagle.Value).ShouldBeTrue();
    }

    [Fact]
    public void IsValid_When_CatBreedValueAndTypeIsCat_Returns_True()
    {
        Breed.IsValid(PetType.Cat, CatBreed.Siamese.Value).ShouldBeTrue();
    }

    [Fact]
    public void IsValid_When_ValueOutOfRangeForType_Returns_False()
    {
        Breed.IsValid(PetType.Dog, 9999).ShouldBeFalse();
    }

    // -----------------------------------------------------------------------
    // NameFor
    // -----------------------------------------------------------------------

    [Fact]
    public void NameFor_When_TypeIsDog_Returns_DogBreedName()
    {
        Breed.NameFor(PetType.Dog, DogBreed.Beagle.Value).ShouldBe(DogBreed.Beagle.Name);
    }

    [Fact]
    public void NameFor_When_TypeIsCat_Returns_CatBreedName()
    {
        Breed.NameFor(PetType.Cat, CatBreed.Siamese.Value).ShouldBe(CatBreed.Siamese.Name);
    }

    // -----------------------------------------------------------------------
    // ListFor
    // -----------------------------------------------------------------------

    [Fact]
    public void ListFor_When_TypeIsDog_Returns_AllDogBreeds_OrderedByValue()
    {
        var result = Breed.ListFor(PetType.Dog);

        result.Count.ShouldBe(DogBreed.List.Count);
        result.Select(b => b.Value).ShouldBe(result.Select(b => b.Value).OrderBy(v => v));
        result.ShouldContain(b => b.Name == DogBreed.Beagle.Name && b.Value == DogBreed.Beagle.Value);
    }

    [Fact]
    public void ListFor_When_TypeIsCat_Returns_AllCatBreeds_OrderedByValue()
    {
        var result = Breed.ListFor(PetType.Cat);

        result.Count.ShouldBe(CatBreed.List.Count);
        result.Select(b => b.Value).ShouldBe(result.Select(b => b.Value).OrderBy(v => v));
    }
}
