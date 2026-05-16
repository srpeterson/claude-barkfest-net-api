using Barkfest.Domain.Entities;
using Barkfest.Domain.Enums;
using Barkfest.Domain.Exceptions;

namespace Barkfest.Domain.Tests;

public class BreedTests
{
    [Fact]
    public void SetDogBreed_Should_Set_Breed_When_Valid()
    {
        var breed = new DogBreedInfo();

        breed.SetDogBreed(DogBreed.Labradoodle);

        breed.DogBreed.ShouldBe(DogBreed.Labradoodle);
    }

    [Fact]
    public void SetDogBreed_Should_Throw_When_Null()
    {
        var breed = new DogBreedInfo();

        Should.Throw<DomainException>(() => breed.SetDogBreed(null!));
    }

    [Fact]
    public void SetCatBreed_Should_Set_Breed_When_Valid()
    {
        var breed = new CatBreedInfo();

        breed.SetCatBreed(CatBreed.MaineCoon);

        breed.CatBreed.ShouldBe(CatBreed.MaineCoon);
    }

    [Fact]
    public void SetCatBreed_Should_Throw_When_Null()
    {
        var breed = new CatBreedInfo();

        Should.Throw<DomainException>(() => breed.SetCatBreed(null!));
    }
}
