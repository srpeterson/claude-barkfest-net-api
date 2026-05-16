using Barkfest.Domain.Entities;
using Barkfest.Domain.Enums;
using Barkfest.Domain.Exceptions;

namespace Barkfest.Domain.Tests;

public class PetTests
{
    private static Pet CreatePet(PetType? petType = null)
    {
        var pet = new Pet(Guid.NewGuid());
        pet.SetPetType(petType ?? PetType.Dog);
        return pet;
    }

    // SetName

    [Fact]
    public void SetName_Should_Set_Name_When_Valid()
    {
        var pet = CreatePet();
        pet.SetName("Buddy");
        pet.Name.ShouldBe("Buddy");
    }

    [Fact]
    public void SetName_Should_Trim_Whitespace()
    {
        var pet = CreatePet();
        pet.SetName("  Buddy  ");
        pet.Name.ShouldBe("Buddy");
    }

    [Fact]
    public void SetName_Should_Accept_Name_At_Max_Length()
    {
        var pet = CreatePet();
        var name = new string('A', Pet.NameMaxLength);
        Should.NotThrow(() => pet.SetName(name));
    }

    [Fact]
    public void SetName_Should_Throw_When_Null()
    {
        var pet = CreatePet();
        Should.Throw<DomainException>(() => pet.SetName(null!));
    }

    [Fact]
    public void SetName_Should_Throw_When_Empty()
    {
        var pet = CreatePet();
        Should.Throw<DomainException>(() => pet.SetName(string.Empty));
    }

    [Fact]
    public void SetName_Should_Throw_When_Whitespace()
    {
        var pet = CreatePet();
        Should.Throw<DomainException>(() => pet.SetName("   "));
    }

    [Fact]
    public void SetName_Should_Throw_When_Exceeds_Max_Length()
    {
        var pet = CreatePet();
        var name = new string('A', Pet.NameMaxLength + 1);
        Should.Throw<DomainException>(() => pet.SetName(name));
    }

    // SetDescription

    [Fact]
    public void SetDescription_Should_Set_Description_When_Valid()
    {
        var pet = CreatePet();
        pet.SetDescription("A friendly dog.");
        pet.Description.ShouldBe("A friendly dog.");
    }

    [Fact]
    public void SetDescription_Should_Trim_Whitespace()
    {
        var pet = CreatePet();
        pet.SetDescription("  A friendly dog.  ");
        pet.Description.ShouldBe("A friendly dog.");
    }

    [Fact]
    public void SetDescription_Should_Accept_Null()
    {
        var pet = CreatePet();
        Should.NotThrow(() => pet.SetDescription(null));
        pet.Description.ShouldBeNull();
    }

    // SetDateOfBirth

    [Fact]
    public void SetDateOfBirth_Should_Set_Date_When_Valid()
    {
        var pet = CreatePet();
        var dob = new DateOnly(2020, 1, 1);
        pet.SetDateOfBirth(dob);
        pet.DateOfBirth.ShouldBe(dob);
    }

    [Fact]
    public void SetDateOfBirth_Should_Accept_Null()
    {
        var pet = CreatePet();
        Should.NotThrow(() => pet.SetDateOfBirth(null));
        pet.DateOfBirth.ShouldBeNull();
    }

    [Fact]
    public void SetDateOfBirth_Should_Throw_When_Future_Date()
    {
        var pet = CreatePet();
        var future = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        Should.Throw<DomainException>(() => pet.SetDateOfBirth(future));
    }

    // Age

    [Fact]
    public void Age_Should_Be_Null_When_No_DateOfBirth()
    {
        var pet = CreatePet();
        pet.Age.ShouldBeNull();
    }

    [Fact]
    public void Age_Should_Be_Correct_When_DateOfBirth_Set()
    {
        var pet = CreatePet();
        var dob = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-3));
        pet.SetDateOfBirth(dob);
        pet.Age.ShouldBe(3);
    }

    [Fact]
    public void Age_Should_Be_Zero_When_DateOfBirth_Is_Today()
    {
        var pet = CreatePet();
        pet.SetDateOfBirth(DateOnly.FromDateTime(DateTime.UtcNow));
        pet.Age.ShouldBe(0);
    }

    // SetPetType

    [Fact]
    public void SetPetType_Should_Set_Type_When_Valid()
    {
        var pet = new Pet(Guid.NewGuid());
        pet.SetPetType(PetType.Cat);
        pet.PetType.ShouldBe(PetType.Cat);
    }

    [Fact]
    public void SetPetType_Should_Throw_When_Null()
    {
        var pet = new Pet(Guid.NewGuid());
        Should.Throw<DomainException>(() => pet.SetPetType(null!));
    }

    // SetBreed

    [Fact]
    public void SetBreed_Should_Set_Dog_Breed_For_Dog()
    {
        var pet = CreatePet(PetType.Dog);
        var breed = new DogBreedInfo();
        breed.SetDogBreed(DogBreed.Beagle);

        pet.SetBreed(breed);

        pet.Breed.ShouldBe(breed);
    }

    [Fact]
    public void SetBreed_Should_Set_Cat_Breed_For_Cat()
    {
        var pet = CreatePet(PetType.Cat);
        var breed = new CatBreedInfo();
        breed.SetCatBreed(CatBreed.Siamese);

        pet.SetBreed(breed);

        pet.Breed.ShouldBe(breed);
    }

    [Fact]
    public void SetBreed_Should_Accept_Null()
    {
        var pet = CreatePet(PetType.Dog);
        Should.NotThrow(() => pet.SetBreed(null));
        pet.Breed.ShouldBeNull();
    }

    [Fact]
    public void SetBreed_Should_Throw_When_Dog_Given_Cat_Breed()
    {
        var pet = CreatePet(PetType.Dog);
        var breed = new CatBreedInfo();
        breed.SetCatBreed(CatBreed.Bengal);

        Should.Throw<DomainException>(() => pet.SetBreed(breed));
    }

    [Fact]
    public void SetBreed_Should_Throw_When_Cat_Given_Dog_Breed()
    {
        var pet = CreatePet(PetType.Cat);
        var breed = new DogBreedInfo();
        breed.SetDogBreed(DogBreed.Poodle);

        Should.Throw<DomainException>(() => pet.SetBreed(breed));
    }

    [Fact]
    public void SetBreed_Should_Throw_When_Other_Given_Any_Breed()
    {
        var pet = CreatePet(PetType.Other);
        var breed = new DogBreedInfo();
        breed.SetDogBreed(DogBreed.Boxer);

        Should.Throw<DomainException>(() => pet.SetBreed(breed));
    }

    // SetProfileImage / RemoveProfileImage

    [Fact]
    public void SetProfileImage_Should_Set_Image_When_Valid()
    {
        var pet = CreatePet();
        pet.SetProfileImage("images/pet.jpg", "image/jpeg");
        pet.ProfileImage.ShouldNotBeNull();
    }

    [Fact]
    public void SetProfileImage_Should_Throw_When_BlobName_Is_Null()
    {
        var pet = CreatePet();
        Should.Throw<DomainException>(() => pet.SetProfileImage(null!, "image/jpeg"));
    }

    [Fact]
    public void SetProfileImage_Should_Throw_When_BlobName_Is_Empty()
    {
        var pet = CreatePet();
        Should.Throw<DomainException>(() => pet.SetProfileImage(string.Empty, "image/jpeg"));
    }

    [Fact]
    public void SetProfileImage_Should_Throw_When_ContentType_Is_Null()
    {
        var pet = CreatePet();
        Should.Throw<DomainException>(() => pet.SetProfileImage("images/pet.jpg", null!));
    }

    [Fact]
    public void SetProfileImage_Should_Throw_When_ContentType_Is_Empty()
    {
        var pet = CreatePet();
        Should.Throw<DomainException>(() => pet.SetProfileImage("images/pet.jpg", string.Empty));
    }

    [Fact]
    public void SetProfileImage_Should_Throw_When_ContentType_Is_Not_Supported()
    {
        var pet = CreatePet();
        Should.Throw<DomainException>(() => pet.SetProfileImage("images/pet.jpg", "image/webp"));
    }

    [Fact]
    public void RemoveProfileImage_Should_Clear_Profile_Image()
    {
        var pet = CreatePet();
        pet.SetProfileImage("images/pet.jpg", "image/jpeg");

        pet.RemoveProfileImage();

        pet.ProfileImage.ShouldBeNull();
    }

    // AddImage / RemoveImage

    [Fact]
    public void AddImage_Should_Add_Image_When_Under_Limit()
    {
        var pet = CreatePet();
        var image = new PetImage();
        image.SetImage("images/1.jpg", "image/jpeg");

        pet.AddImage(image);

        pet.Images.Count.ShouldBe(1);
    }

    [Fact]
    public void AddImage_Should_Accept_Up_To_Max_Images()
    {
        var pet = CreatePet();

        for (var i = 0; i < Pet.MaxImages; i++)
        {
            var image = new PetImage();
            image.SetImage($"images/{i}.jpg", "image/jpeg");
            pet.AddImage(image);
        }

        pet.Images.Count.ShouldBe(Pet.MaxImages);
    }

    [Fact]
    public void AddImage_Should_Throw_When_Max_Images_Exceeded()
    {
        var pet = CreatePet();

        for (var i = 0; i < Pet.MaxImages; i++)
        {
            var image = new PetImage();
            image.SetImage($"images/{i}.jpg", "image/jpeg");
            pet.AddImage(image);
        }

        var extra = new PetImage();
        extra.SetImage("images/extra.jpg", "image/jpeg");

        Should.Throw<DomainException>(() => pet.AddImage(extra));
    }

    [Fact]
    public void AddImage_Should_Throw_When_Null()
    {
        var pet = CreatePet();
        Should.Throw<DomainException>(() => pet.AddImage(null!));
    }

    [Fact]
    public void RemoveImage_Should_Remove_Image_When_Found()
    {
        var pet = CreatePet();
        var image = new PetImage();
        image.SetImage("images/1.jpg", "image/jpeg");
        pet.AddImage(image);

        pet.RemoveImage(image.Id);

        pet.Images.Count.ShouldBe(0);
    }

    [Fact]
    public void RemoveImage_Should_Throw_When_Not_Found()
    {
        var pet = CreatePet();

        Should.Throw<DomainException>(() => pet.RemoveImage(Guid.NewGuid()));
    }
}
