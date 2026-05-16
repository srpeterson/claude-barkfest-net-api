using Barkfest.Domain.Enums;

namespace Barkfest.Domain.Tests;

public class PetTypeTests
{
    [Fact]
    public void PetType_Should_Have_Dog_Value()
    {
        PetType.Dog.ShouldNotBeNull();
        PetType.Dog.Value.ShouldBe(1);
    }

    [Fact]
    public void PetType_Should_Have_Cat_Value()
    {
        PetType.Cat.ShouldNotBeNull();
        PetType.Cat.Value.ShouldBe(2);
    }

    [Fact]
    public void PetType_Should_Have_Other_Value()
    {
        PetType.Other.ShouldNotBeNull();
        PetType.Other.Value.ShouldBe(3);
    }

    [Fact]
    public void PetType_Should_Have_Exactly_3_Values()
    {
        PetType.List.Count.ShouldBe(3);
    }

    [Theory]
    [InlineData("Dog")]
    [InlineData("Cat")]
    [InlineData("Other")]
    public void PetType_Should_Support_Lookup_By_Name(string name)
    {
        var result = PetType.FromName(name);

        result.ShouldNotBeNull();
        result.Name.ShouldBe(name);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void PetType_Should_Support_Lookup_By_Value(int value)
    {
        var result = PetType.FromValue(value);

        result.ShouldNotBeNull();
        result.Value.ShouldBe(value);
    }

    [Fact]
    public void PetType_Should_Throw_When_Looking_Up_Invalid_Name()
    {
        Should.Throw<Exception>(() => PetType.FromName("Invalid"));
    }

    [Fact]
    public void PetType_Should_Throw_When_Looking_Up_Invalid_Value()
    {
        Should.Throw<Exception>(() => PetType.FromValue(99));
    }
}
