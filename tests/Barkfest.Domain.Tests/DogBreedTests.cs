using Barkfest.Domain.Enums;

namespace Barkfest.Domain.Tests;

public class DogBreedTests
{
    [Fact]
    public void DogBreed_Should_Have_Exactly_30_Values()
    {
        DogBreed.List.Count.ShouldBe(30);
    }

    [Fact]
    public void DogBreed_Should_Include_Labradoodle()
    {
        DogBreed.List.ShouldContain(b => b.Name == "Labradoodle");
    }

    [Fact]
    public void DogBreed_Should_Include_Goldendoodle()
    {
        DogBreed.List.ShouldContain(b => b.Name == "Goldendoodle");
    }

    [Fact]
    public void DogBreed_Should_Include_Cockapoo()
    {
        DogBreed.List.ShouldContain(b => b.Name == "Cockapoo");
    }

    [Fact]
    public void DogBreed_Should_Include_Mixed()
    {
        DogBreed.List.ShouldContain(b => b.Name == "Mixed");
    }

    [Fact]
    public void DogBreed_Should_Include_Other()
    {
        DogBreed.List.ShouldContain(b => b.Name == "Other");
    }

    [Fact]
    public void DogBreed_Should_Support_Lookup_By_Name()
    {
        var result = DogBreed.FromName("French Bulldog");

        result.ShouldNotBeNull();
        result.Name.ShouldBe("French Bulldog");
    }

    [Fact]
    public void DogBreed_Should_Support_Lookup_By_Value()
    {
        var result = DogBreed.FromValue(1);

        result.ShouldNotBeNull();
        result.Value.ShouldBe(1);
    }

    [Fact]
    public void DogBreed_Should_Throw_When_Looking_Up_Invalid_Name()
    {
        Should.Throw<Exception>(() => DogBreed.FromName("Invalid Breed"));
    }

    [Fact]
    public void DogBreed_Should_Throw_When_Looking_Up_Invalid_Value()
    {
        Should.Throw<Exception>(() => DogBreed.FromValue(99));
    }

    [Fact]
    public void DogBreed_Values_Should_Be_Unique()
    {
        var values = DogBreed.List.Select(b => b.Value).ToList();

        values.Distinct().Count().ShouldBe(values.Count);
    }

    [Fact]
    public void DogBreed_Names_Should_Be_Unique()
    {
        var names = DogBreed.List.Select(b => b.Name).ToList();

        names.Distinct().Count().ShouldBe(names.Count);
    }
}
