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
    public void SetName_ValidName_SetsTrimmedValue()
    {
        var pet = BuildPet();

        pet.SetName("  Buddy  ");

        pet.Name.ShouldBe("Buddy");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void SetName_EmptyOrWhitespace_ThrowsDomainException(string name)
    {
        var pet = BuildPet();

        Should.Throw<DomainException>(() => pet.SetName(name))
            .Message.ShouldBe("Name is required.");
    }

    [Fact]
    public void SetName_ExceedsMaxLength_ThrowsDomainException()
    {
        var pet = BuildPet();
        var longName = new string('X', Pet.NameMaxLength + 1);

        Should.Throw<DomainException>(() => pet.SetName(longName))
            .Message.ShouldContain(Pet.NameMaxLength.ToString());
    }

    [Fact]
    public void SetName_ExactlyMaxLength_Succeeds()
    {
        var pet = BuildPet();
        var name = new string('X', Pet.NameMaxLength);

        pet.SetName(name);

        pet.Name.ShouldBe(name);
    }

    // -----------------------------------------------------------------------
    // SetDescription
    // -----------------------------------------------------------------------

    [Fact]
    public void SetDescription_ValidValue_SetsTrimmedValue()
    {
        var pet = BuildPet();

        pet.SetDescription("  Very good boy  ");

        pet.Description.ShouldBe("Very good boy");
    }

    [Fact]
    public void SetDescription_Null_SetsNull()
    {
        var pet = BuildPet();

        pet.SetDescription(null);

        pet.Description.ShouldBeNull();
    }

    // -----------------------------------------------------------------------
    // SetDateOfBirth
    // -----------------------------------------------------------------------

    [Fact]
    public void SetDateOfBirth_PastDate_SetsValue()
    {
        var pet = BuildPet();
        var dob = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-2));

        pet.SetDateOfBirth(dob);

        pet.DateOfBirth.ShouldBe(dob);
    }

    [Fact]
    public void SetDateOfBirth_Today_Succeeds()
    {
        var pet = BuildPet();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        pet.SetDateOfBirth(today);

        pet.DateOfBirth.ShouldBe(today);
    }

    [Fact]
    public void SetDateOfBirth_FutureDate_ThrowsDomainException()
    {
        var pet = BuildPet();
        var future = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

        Should.Throw<DomainException>(() => pet.SetDateOfBirth(future))
            .Message.ShouldBe("Date of birth cannot be in the future.");
    }

    [Fact]
    public void SetDateOfBirth_Null_ClearsValue()
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
    public void Age_WhenDateOfBirthIsNull_ReturnsNull()
    {
        var pet = BuildPet();

        pet.Age.ShouldBeNull();
    }

    [Fact]
    public void Age_WhenDateOfBirthSet_ReturnsCorrectAge()
    {
        var pet = BuildPet();
        var dob = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-3));
        pet.SetDateOfBirth(dob);

        pet.Age.ShouldBe(3);
    }

    [Fact]
    public void Age_BeforeBirthdayThisYear_ReturnsOneLess()
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
    public void SetPetType_ValidType_SetsValue()
    {
        var pet = BuildPet();

        pet.SetPetType(PetType.Dog);

        pet.PetType.ShouldBe(PetType.Dog);
    }

    [Fact]
    public void SetPetType_Null_ThrowsDomainException()
    {
        var pet = BuildPet();

        Should.Throw<DomainException>(() => pet.SetPetType(null!))
            .Message.ShouldBe("Pet type is required.");
    }

    // -----------------------------------------------------------------------
    // SetBreed
    // -----------------------------------------------------------------------

    [Fact]
    public void SetBreed_Null_ClearsBreed()
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
    public void SetBreed_DogBreedForDogType_SetsBreed()
    {
        var pet = BuildPet();
        pet.SetPetType(PetType.Dog);
        var dogBreed = new DogBreedInfo();
        dogBreed.SetDogBreed(DogBreed.GoldenRetriever);

        pet.SetBreed(dogBreed);

        pet.Breed.ShouldBe(dogBreed);
    }

    [Fact]
    public void SetBreed_CatBreedForDogType_ThrowsDomainException()
    {
        var pet = BuildPet();
        pet.SetPetType(PetType.Dog);
        var catBreed = new CatBreedInfo();

        Should.Throw<DomainException>(() => pet.SetBreed(catBreed))
            .Message.ShouldBe("Dog pet type requires a dog breed.");
    }

    [Fact]
    public void SetBreed_CatBreedForCatType_SetsBreed()
    {
        var pet = BuildPet();
        pet.SetPetType(PetType.Cat);
        var catBreed = new CatBreedInfo();
        catBreed.SetCatBreed(CatBreed.Siamese);

        pet.SetBreed(catBreed);

        pet.Breed.ShouldBe(catBreed);
    }

    [Fact]
    public void SetBreed_DogBreedForCatType_ThrowsDomainException()
    {
        var pet = BuildPet();
        pet.SetPetType(PetType.Cat);
        var dogBreed = new DogBreedInfo();

        Should.Throw<DomainException>(() => pet.SetBreed(dogBreed))
            .Message.ShouldBe("Cat pet type requires a cat breed.");
    }

    [Fact]
    public void SetBreed_AnyBreedForOtherType_ThrowsDomainException()
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
    public void SetProfileImage_ValidArgs_SetsProfileImage()
    {
        var pet = BuildPet();

        pet.SetProfileImage("pets/abc/profile.jpg", "image/jpeg");

        pet.ProfileImage.ShouldNotBeNull();
        pet.ProfileImage!.BlobName.ShouldBe("pets/abc/profile.jpg");
    }

    [Fact]
    public void RemoveProfileImage_WhenSet_ClearsProfileImage()
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
    public void AddImage_BelowLimit_AddsImage()
    {
        var pet = BuildPet();
        var image = BuildPetImage();

        pet.AddImage(image);

        pet.Images.Count.ShouldBe(1);
    }

    [Fact]
    public void AddImage_AtMaxLimit_ThrowsDomainException()
    {
        var pet = BuildPet();
        for (var i = 0; i < Pet.MaxImages; i++)
            pet.AddImage(BuildPetImage(i));

        Should.Throw<DomainException>(() => pet.AddImage(BuildPetImage()))
            .Message.ShouldContain(Pet.MaxImages.ToString());
    }

    [Fact]
    public void AddImage_Null_ThrowsDomainException()
    {
        var pet = BuildPet();

        Should.Throw<DomainException>(() => pet.AddImage(null!))
            .Message.ShouldBe("Image is required.");
    }

    // -----------------------------------------------------------------------
    // RemoveImage
    // -----------------------------------------------------------------------

    [Fact]
    public void RemoveImage_ExistingImage_RemovesIt()
    {
        var pet = BuildPet();
        var image = BuildPetImage();
        pet.AddImage(image);

        pet.RemoveImage(image.Id);

        pet.Images.ShouldBeEmpty();
    }

    [Fact]
    public void RemoveImage_UnknownId_ThrowsDomainException()
    {
        var pet = BuildPet();

        Should.Throw<DomainException>(() => pet.RemoveImage(Guid.NewGuid()))
            .Message.ShouldBe("Image not found.");
    }

    // -----------------------------------------------------------------------
    // Id / OwnerId
    // -----------------------------------------------------------------------

    [Fact]
    public void NewPet_HasNonEmptyId()
    {
        var pet = BuildPet();

        pet.Id.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public void NewPet_OwnerIdIsSet()
    {
        var ownerId = Guid.NewGuid();
        var pet = new Pet(ownerId);

        pet.OwnerId.ShouldBe(ownerId);
    }

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private static PetImage BuildPetImage(int displayOrder = 0)
    {
        var image = new PetImage();
        image.SetImage("pets/abc/gallery/photo.jpg", "image/jpeg");
        image.SetDisplayOrder(displayOrder);
        return image;
    }
}
