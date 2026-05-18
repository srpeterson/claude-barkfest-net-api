using Barkfest.Domain.Entities;
using Barkfest.Domain.Enums;
using Barkfest.Domain.Exceptions;

namespace Barkfest.Domain.Tests;

public class BreedTests
{
    [Fact]
    public void SetDogBreed_When_BreedIsValid_Sets_Breed()
    {
        var breed = new DogBreedInfo();

        breed.SetDogBreed(DogBreed.Labradoodle);

        breed.DogBreed.ShouldBe(DogBreed.Labradoodle);
    }

    [Fact]
    public void SetDogBreed_When_Null_Throws_DomainException()
    {
        var breed = new DogBreedInfo();

        Should.Throw<DomainException>(() => breed.SetDogBreed(null!));
    }

    [Fact]
    public void SetCatBreed_When_BreedIsValid_Sets_Breed()
    {
        var breed = new CatBreedInfo();

        breed.SetCatBreed(CatBreed.MaineCoon);

        breed.CatBreed.ShouldBe(CatBreed.MaineCoon);
    }

    [Fact]
    public void SetCatBreed_When_Null_Throws_DomainException()
    {
        var breed = new CatBreedInfo();

        Should.Throw<DomainException>(() => breed.SetCatBreed(null!));
    }
}
