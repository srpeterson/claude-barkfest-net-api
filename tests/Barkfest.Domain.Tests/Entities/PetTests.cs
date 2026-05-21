using Barkfest.Domain.Entities;
using Barkfest.Domain.Enums;
using Barkfest.Domain.Exceptions;

namespace Barkfest.Domain.Tests.Entities;

public class PetTests
{
    private static Pet BuildPet() =>
        new Pet(Guid.NewGuid());

    // -----------------------------------------------------------------------
    // SetName
    // -----------------------------------------------------------------------

    [Fact]
    public void SetName_When_NameIsValid_Sets_TrimmedName()
    {
        var pet = BuildPet();

        pet.SetName("  Buddy  ");

        pet.Name.ShouldBe("Buddy");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void SetName_When_EmptyOrWhitespace_Throws_DomainException(string name)
    {
        var pet = BuildPet();

        Should.Throw<DomainException>(() => pet.SetName(name))
            .Message.ShouldBe("Name is required.");
    }

    [Fact]
    public void SetName_When_ExceedsMaxLength_Throws_DomainException()
    {
        var pet = BuildPet();
        var longName = new string('X', Pet.NameMaxLength + 1);

        Should.Throw<DomainException>(() => pet.SetName(longName))
            .Message.ShouldContain(Pet.NameMaxLength.ToString());
    }

    // -----------------------------------------------------------------------
    // SetDescription
    // -----------------------------------------------------------------------

    [Fact]
    public void SetDescription_When_ValueIsValid_Sets_TrimmedDescription()
    {
        var pet = BuildPet();

        pet.SetDescription("  Very good boy  ");

        pet.Description.ShouldBe("Very good boy");
    }

    [Fact]
    public void SetDescription_When_Null_Sets_Null()
    {
        var pet = BuildPet();

        pet.SetDescription(null);

        pet.Description.ShouldBeNull();
    }

    // -----------------------------------------------------------------------
    // SetDateOfBirth
    // -----------------------------------------------------------------------

    [Fact]
    public void SetDateOfBirth_When_DateIsValid_Sets_Value()
    {
        var pet = BuildPet();
        var dob = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-2));

        pet.SetDateOfBirth(dob);

        pet.DateOfBirth.ShouldBe(dob);
    }

    [Fact]
    public void SetDateOfBirth_When_DateIsInFuture_Throws_DomainException()
    {
        var pet = BuildPet();
        var future = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

        Should.Throw<DomainException>(() => pet.SetDateOfBirth(future))
            .Message.ShouldBe("Date of birth cannot be in the future.");
    }

    [Fact]
    public void SetDateOfBirth_When_Null_Clears_Value()
    {
        var pet = BuildPet();
        pet.SetDateOfBirth(DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1)));

        pet.SetDateOfBirth(null);

        pet.DateOfBirth.ShouldBeNull();
    }

    // -----------------------------------------------------------------------
    // Age
    // -----------------------------------------------------------------------

    [Fact]
    public void Age_When_DateOfBirthIsNull_Returns_Null()
    {
        var pet = BuildPet();

        pet.Age.ShouldBeNull();
    }

    [Fact]
    public void Age_When_DateOfBirthIsSet_Returns_CorrectAge()
    {
        var pet = BuildPet();
        var dob = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-3));
        pet.SetDateOfBirth(dob);

        pet.Age.ShouldBe(3);
    }

    [Fact]
    public void Age_When_BirthdayHasNotPassedThisYear_Returns_AgeAtLastBirthday()
    {
        var pet = BuildPet();
        // Born exactly 3 years ago but bump forward 1 day so birthday hasn't passed yet
        var dob = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-3).AddDays(1));
        pet.SetDateOfBirth(dob);

        // Age should be 2, not 3
        pet.Age.ShouldBe(2);
    }

    // -----------------------------------------------------------------------
    // SetPetType
    // -----------------------------------------------------------------------

    [Fact]
    public void SetPetType_When_TypeIsValid_Sets_Value()
    {
        var pet = BuildPet();

        pet.SetPetType(PetType.Dog);

        pet.PetType.ShouldBe(PetType.Dog);
    }

    [Fact]
    public void SetPetType_When_Null_Throws_DomainException()
    {
        var pet = BuildPet();

        Should.Throw<DomainException>(() => pet.SetPetType(null!))
            .Message.ShouldBe("Pet type is required.");
    }

    // -----------------------------------------------------------------------
    // SetBreed
    // -----------------------------------------------------------------------

    [Fact]
    public void SetBreed_When_Null_Throws_DomainException()
    {
        var pet = BuildPet();
        pet.SetPetType(PetType.Dog);

        Should.Throw<DomainException>(() => pet.SetBreed(null!))
            .Message.ShouldBe("Breed is required.");
    }

    [Fact]
    public void SetBreed_When_DogBreedAndTypeIsDog_Sets_Breed()
    {
        var pet = BuildPet();
        pet.SetPetType(PetType.Dog);
        var dogBreed = new DogBreedInfo();
        dogBreed.SetDogBreed(DogBreed.GoldenRetriever);

        pet.SetBreed(dogBreed);

        pet.Breed.ShouldBe(dogBreed);
    }

    [Fact]
    public void SetBreed_When_CatBreedAndTypeIsDog_Throws_DomainException()
    {
        var pet = BuildPet();
        pet.SetPetType(PetType.Dog);
        var catBreed = new CatBreedInfo();

        Should.Throw<DomainException>(() => pet.SetBreed(catBreed))
            .Message.ShouldBe("Dog pet type requires a dog breed.");
    }

    [Fact]
    public void SetBreed_When_CatBreedAndTypeIsCat_Sets_Breed()
    {
        var pet = BuildPet();
        pet.SetPetType(PetType.Cat);
        var catBreed = new CatBreedInfo();
        catBreed.SetCatBreed(CatBreed.Siamese);

        pet.SetBreed(catBreed);

        pet.Breed.ShouldBe(catBreed);
    }

    [Fact]
    public void SetBreed_When_DogBreedAndTypeIsCat_Throws_DomainException()
    {
        var pet = BuildPet();
        pet.SetPetType(PetType.Cat);
        var dogBreed = new DogBreedInfo();

        Should.Throw<DomainException>(() => pet.SetBreed(dogBreed))
            .Message.ShouldBe("Cat pet type requires a cat breed.");
    }

    // -----------------------------------------------------------------------
    // SetFeaturedImage
    // -----------------------------------------------------------------------

    [Fact]
    public void SetFeaturedImage_When_ImageExists_Sets_IsFeaturedImageTrue()
    {
        var pet = BuildPet();
        var image = new PetImageBuilder().Build();
        pet.AddImage(image);

        pet.SetFeaturedImage(image.Id);

        pet.FeaturedImage.ShouldNotBeNull();
        pet.FeaturedImage!.Id.ShouldBe(image.Id);
        image.IsFeaturedImage.ShouldBeTrue();
    }

    [Fact]
    public void SetFeaturedImage_When_AlreadyFeaturedImageExists_Replaces_Featured()
    {
        var pet = BuildPet();
        var first = new PetImageBuilder().WithDisplayOrder(0).Build();
        var second = new PetImageBuilder().WithDisplayOrder(1).Build();
        pet.AddImage(first);
        pet.AddImage(second);
        pet.SetFeaturedImage(first.Id);

        pet.SetFeaturedImage(second.Id);

        first.IsFeaturedImage.ShouldBeFalse();
        second.IsFeaturedImage.ShouldBeTrue();
        pet.FeaturedImage!.Id.ShouldBe(second.Id);
    }

    [Fact]
    public void SetFeaturedImage_When_ImageNotFound_Throws_DomainException()
    {
        var pet = BuildPet();

        Should.Throw<DomainException>(() => pet.SetFeaturedImage(Guid.NewGuid()))
            .Message.ShouldBe("Image not found.");
    }

    [Fact]
    public void FeaturedImage_When_NoImages_Returns_Null()
    {
        var pet = BuildPet();

        pet.FeaturedImage.ShouldBeNull();
    }

    // -----------------------------------------------------------------------
    // AddImage
    // -----------------------------------------------------------------------

    [Fact]
    public void AddImage_When_BelowMaxLimit_Adds_Image()
    {
        var pet = BuildPet();
        var image = new PetImageBuilder().Build();

        pet.AddImage(image);

        pet.Images.Count.ShouldBe(1);
    }

    [Fact]
    public void AddImage_When_ExceedsMaxLimit_Throws_DomainException()
    {
        var pet = BuildPet();
        for (var i = 0; i < Pet.MaxImages; i++)
            pet.AddImage(new PetImageBuilder().WithDisplayOrder(i).Build());

        Should.Throw<DomainException>(() => pet.AddImage(new PetImageBuilder().Build()))
            .Message.ShouldContain(Pet.MaxImages.ToString());
    }

    [Fact]
    public void AddImage_When_Null_Throws_DomainException()
    {
        var pet = BuildPet();

        Should.Throw<DomainException>(() => pet.AddImage(null!))
            .Message.ShouldBe("Image is required.");
    }

    [Fact]
    public void AddImage_When_FirstImage_AutoSets_IsFeaturedImage()
    {
        var pet = BuildPet();
        var image = new PetImageBuilder().Build();

        pet.AddImage(image);

        image.IsFeaturedImage.ShouldBeTrue();
        pet.FeaturedImage!.Id.ShouldBe(image.Id);
    }

    [Fact]
    public void AddImage_When_SecondImage_DoesNotAutoFeature()
    {
        var pet = BuildPet();
        var first = new PetImageBuilder().WithDisplayOrder(0).Build();
        var second = new PetImageBuilder().WithDisplayOrder(1).Build();
        pet.AddImage(first);

        pet.AddImage(second);

        second.IsFeaturedImage.ShouldBeFalse();
        pet.FeaturedImage!.Id.ShouldBe(first.Id);
    }

    // -----------------------------------------------------------------------
    // RemoveImage
    // -----------------------------------------------------------------------

    [Fact]
    public void RemoveImage_When_ImageExists_Removes_Image()
    {
        var pet = BuildPet();
        var image = new PetImageBuilder().Build();
        pet.AddImage(image);

        pet.RemoveImage(image.Id);

        pet.Images.ShouldBeEmpty();
    }

    [Fact]
    public void RemoveImage_When_IdNotFound_Throws_DomainException()
    {
        var pet = BuildPet();

        Should.Throw<DomainException>(() => pet.RemoveImage(Guid.NewGuid()))
            .Message.ShouldBe("Image not found.");
    }

    // -----------------------------------------------------------------------
    // RemoveImages (batch)
    // -----------------------------------------------------------------------

    [Fact]
    public void RemoveImages_When_AllExist_Removes_All()
    {
        var pet = BuildPet();
        var first = new PetImageBuilder().WithDisplayOrder(0).Build();
        var second = new PetImageBuilder().WithDisplayOrder(1).Build();
        var third = new PetImageBuilder().WithDisplayOrder(2).Build();
        pet.AddImage(first);
        pet.AddImage(second);
        pet.AddImage(third);

        pet.RemoveImages([first.Id, second.Id]);

        pet.Images.Count.ShouldBe(1);
        pet.Images.ShouldContain(i => i.Id == third.Id);
    }

    [Fact]
    public void RemoveImages_When_AnyNotFound_Throws_DomainException()
    {
        var pet = BuildPet();
        var image = new PetImageBuilder().Build();
        pet.AddImage(image);

        Should.Throw<DomainException>(() => pet.RemoveImages([image.Id, Guid.NewGuid()]))
            .Message.ShouldBe("One or more images were not found.");
    }

    [Fact]
    public void RemoveImages_When_FeaturedImageIncluded_FeaturedImage_Becomes_Null()
    {
        var pet = BuildPet();
        var first = new PetImageBuilder().WithDisplayOrder(0).Build();
        var second = new PetImageBuilder().WithDisplayOrder(1).Build();
        pet.AddImage(first);
        pet.AddImage(second);

        pet.RemoveImages([first.Id]);

        pet.FeaturedImage.ShouldBeNull();
    }

    // -----------------------------------------------------------------------
    // Id / OwnerId
    // -----------------------------------------------------------------------

    [Fact]
    public void NewPet_When_Instantiated_Returns_NonEmptyId()
    {
        var pet = BuildPet();

        pet.Id.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public void NewPet_When_Instantiated_Returns_SetOwnerId()
    {
        var ownerId = Guid.NewGuid();
        var pet = new Pet(ownerId);

        pet.OwnerId.ShouldBe(ownerId);
    }

}
