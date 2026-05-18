using Barkfest.Domain.Enums;

namespace Barkfest.Domain.Tests;

public class DogBreedTests
{
    [Fact]
    public void DogBreed_List_Contains_AllDefinedBreeds()
    {
        DogBreed.List.Count.ShouldBe(30);
    }

    [Fact]
    public void DogBreed_List_Includes_Labradoodle()
    {
        DogBreed.List.ShouldContain(b => b.Name == "Labradoodle");
    }

    [Fact]
    public void DogBreed_List_Includes_Goldendoodle()
    {
        DogBreed.List.ShouldContain(b => b.Name == "Goldendoodle");
    }

    [Fact]
    public void DogBreed_List_Includes_Cockapoo()
    {
        DogBreed.List.ShouldContain(b => b.Name == "Cockapoo");
    }

    [Fact]
    public void DogBreed_List_Includes_Mixed()
    {
        DogBreed.List.ShouldContain(b => b.Name == "Mixed");
    }

    [Fact]
    public void DogBreed_List_Includes_Other()
    {
        DogBreed.List.ShouldContain(b => b.Name == "Other");
    }

    [Fact]
    public void FromName_When_NameIsValid_Returns_DogBreed()
    {
        var result = DogBreed.FromName("French Bulldog");

        result.ShouldNotBeNull();
        result.Name.ShouldBe("French Bulldog");
    }

    [Fact]
    public void FromValue_When_ValueIsValid_Returns_DogBreed()
    {
        var result = DogBreed.FromValue(1);

        result.ShouldNotBeNull();
        result.Value.ShouldBe(1);
    }

    [Fact]
    public void FromName_When_NameIsInvalid_Throws_Exception()
    {
        Should.Throw<Exception>(() => DogBreed.FromName("Invalid Breed"));
    }

    [Fact]
    public void FromValue_When_ValueIsInvalid_Throws_Exception()
    {
        Should.Throw<Exception>(() => DogBreed.FromValue(99));
    }

    [Fact]
    public void DogBreed_Values_Are_Unique()
    {
        var values = DogBreed.List.Select(b => b.Value).ToList();

        values.Distinct().Count().ShouldBe(values.Count);
    }

    [Fact]
    public void DogBreed_Names_Are_Unique()
    {
        var names = DogBreed.List.Select(b => b.Name).ToList();

        names.Distinct().Count().ShouldBe(names.Count);
    }
}
