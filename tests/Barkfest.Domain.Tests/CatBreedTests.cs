using Barkfest.Domain.Enums;

namespace Barkfest.Domain.Tests;

public class CatBreedTests
{
    [Fact]
    public void CatBreed_List_Contains_AllDefinedBreeds()
    {
        CatBreed.List.Count.ShouldBe(29);
    }

    [Fact]
    public void CatBreed_List_Includes_DomesticShorthair()
    {
        CatBreed.List.ShouldContain(b => b.Name == "Domestic Shorthair");
    }

    [Fact]
    public void CatBreed_List_Includes_Tabby()
    {
        CatBreed.List.ShouldContain(b => b.Name == "Tabby");
    }

    [Fact]
    public void CatBreed_List_Includes_Mixed()
    {
        CatBreed.List.ShouldContain(b => b.Name == "Mixed");
    }

    [Fact]
    public void CatBreed_List_Includes_Other()
    {
        CatBreed.List.ShouldContain(b => b.Name == "Other");
    }

    [Fact]
    public void FromName_When_NameIsValid_Returns_CatBreed()
    {
        var result = CatBreed.FromName("Maine Coon");

        result.ShouldNotBeNull();
        result.Name.ShouldBe("Maine Coon");
    }

    [Fact]
    public void FromValue_When_ValueIsValid_Returns_CatBreed()
    {
        var result = CatBreed.FromValue(1);

        result.ShouldNotBeNull();
        result.Value.ShouldBe(1);
    }

    [Fact]
    public void FromName_When_NameIsInvalid_Throws_Exception()
    {
        Should.Throw<Exception>(() => CatBreed.FromName("Invalid Breed"));
    }

    [Fact]
    public void FromValue_When_ValueIsInvalid_Throws_Exception()
    {
        Should.Throw<Exception>(() => CatBreed.FromValue(99));
    }

    [Fact]
    public void CatBreed_Values_Are_Unique()
    {
        var values = CatBreed.List.Select(b => b.Value).ToList();

        values.Distinct().Count().ShouldBe(values.Count);
    }

    [Fact]
    public void CatBreed_Names_Are_Unique()
    {
        var names = CatBreed.List.Select(b => b.Name).ToList();

        names.Distinct().Count().ShouldBe(names.Count);
    }
}
