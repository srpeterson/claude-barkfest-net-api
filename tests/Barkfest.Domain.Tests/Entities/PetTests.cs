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
    public void SetBreed_When_Null_Clears_Breed()
    {
        var pet = BuildPet();
        pet.SetPetType(PetType.Dog);
        var dogBreed = new DogBreedInfo();
        dogBreed.SetDogBreed(DogBreed.Beagle);
        pet.SetBreed(dogBreed);

        pet.SetBreed(null);

        pet.Breed.ShouldBeNull();
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

    [Fact]
    public void SetBreed_When_BreedSetAndTypeIsOther_Throws_DomainException()
    {
        var pet = BuildPet();
        pet.SetPetType(PetType.Other);
        var dogBreed = new DogBreedInfo();

        Should.Throw<DomainException>(() => pet.SetBreed(dogBreed))
            .Message.ShouldBe("Other pet type cannot have a breed.");
    }

    // -----------------------------------------------------------------------
    // SetProfileImage / RemoveProfileImage
    // -----------------------------------------------------------------------

    [Fact]
    public void SetProfileImage_When_ArgsAreValid_Sets_ProfileImage()
    {
        var pet = BuildPet();

        pet.SetProfileImage("pets/abc/profile.jpg", "image/jpeg");

        pet.ProfileImage.ShouldNotBeNull();
        pet.ProfileImage!.BlobName.ShouldBe("pets/abc/profile.jpg");
    }

    [Fact]
    public void RemoveProfileImage_When_ImageIsSet_Clears_ProfileImage()
    {
        var pet = BuildPet();
        pet.SetProfileImage("pets/abc/profile.jpg", "image/jpeg");

        pet.RemoveProfileImage();

        pet.ProfileImage.ShouldBeNull();
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
