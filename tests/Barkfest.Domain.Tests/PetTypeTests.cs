using Barkfest.Domain.Enums;

namespace Barkfest.Domain.Tests;

public class PetTypeTests
{
    [Fact]
    public void PetType_Dog_Has_CorrectIntValue()
    {
        PetType.Dog.ShouldNotBeNull();
        PetType.Dog.Value.ShouldBe(1);
    }

    [Fact]
    public void PetType_Cat_Has_CorrectIntValue()
    {
        PetType.Cat.ShouldNotBeNull();
        PetType.Cat.Value.ShouldBe(2);
    }

    [Fact]
    public void PetType_Other_Has_CorrectIntValue()
    {
        PetType.Other.ShouldNotBeNull();
        PetType.Other.Value.ShouldBe(3);
    }

    [Fact]
    public void PetType_List_Contains_AllDefinedTypes()
    {
        PetType.List.Count.ShouldBe(3);
    }

    [Theory]
    [InlineData("Dog")]
    [InlineData("Cat")]
    [InlineData("Other")]
    public void FromName_When_NameIsValid_Returns_PetType(string name)
    {
        var result = PetType.FromName(name);

        result.ShouldNotBeNull();
        result.Name.ShouldBe(name);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void FromValue_When_ValueIsValid_Returns_PetType(int value)
    {
        var result = PetType.FromValue(value);

        result.ShouldNotBeNull();
        result.Value.ShouldBe(value);
    }

    [Fact]
    public void FromName_When_NameIsInvalid_Throws_Exception()
    {
        Should.Throw<Exception>(() => PetType.FromName("Invalid"));
    }

    [Fact]
    public void FromValue_When_ValueIsInvalid_Throws_Exception()
    {
        Should.Throw<Exception>(() => PetType.FromValue(99));
    }
}
