using Barkfest.Domain.Enums;

namespace Barkfest.Domain.Tests;

public class CatBreedTests
{
    [Fact]
    public void CatBreed_Should_Have_Exactly_29_Values()
    {
        CatBreed.List.Count.ShouldBe(29);
    }

    [Fact]
    public void CatBreed_Should_Include_DomesticShorthair()
    {
        CatBreed.List.ShouldContain(b => b.Name == "Domestic Shorthair");
    }

    [Fact]
    public void CatBreed_Should_Include_Tabby()
    {
        CatBreed.List.ShouldContain(b => b.Name == "Tabby");
    }

    [Fact]
    public void CatBreed_Should_Include_Mixed()
    {
        CatBreed.List.ShouldContain(b => b.Name == "Mixed");
    }

    [Fact]
    public void CatBreed_Should_Include_Other()
    {
        CatBreed.List.ShouldContain(b => b.Name == "Other");
    }

    [Fact]
    public void CatBreed_Should_Support_Lookup_By_Name()
    {
        var result = CatBreed.FromName("Maine Coon");

        result.ShouldNotBeNull();
        result.Name.ShouldBe("Maine Coon");
    }

    [Fact]
    public void CatBreed_Should_Support_Lookup_By_Value()
    {
        var result = CatBreed.FromValue(1);

        result.ShouldNotBeNull();
        result.Value.ShouldBe(1);
    }

    [Fact]
    public void CatBreed_Should_Throw_When_Looking_Up_Invalid_Name()
    {
        Should.Throw<Exception>(() => CatBreed.FromName("Invalid Breed"));
    }

    [Fact]
    public void CatBreed_Should_Throw_When_Looking_Up_Invalid_Value()
    {
        Should.Throw<Exception>(() => CatBreed.FromValue(99));
    }

    [Fact]
    public void CatBreed_Values_Should_Be_Unique()
    {
        var values = CatBreed.List.Select(b => b.Value).ToList();

        values.Distinct().Count().ShouldBe(values.Count);
    }

    [Fact]
    public void CatBreed_Names_Should_Be_Unique()
    {
        var names = CatBreed.List.Select(b => b.Name).ToList();

        names.Distinct().Count().ShouldBe(names.Count);
    }
}
