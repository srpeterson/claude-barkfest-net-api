using Barkfest.Domain.Entities;
using Barkfest.Domain.Exceptions;

namespace Barkfest.Domain.Tests;

public class OwnerTests
{
    // SetFirstName

    [Fact]
    public void SetFirstName_Should_Set_Name_When_Valid()
    {
        var owner = new Owner();
        owner.SetFirstName("Alice");
        owner.FirstName.ShouldBe("Alice");
    }

    [Fact]
    public void SetFirstName_Should_Trim_Whitespace()
    {
        var owner = new Owner();
        owner.SetFirstName("  Alice  ");
        owner.FirstName.ShouldBe("Alice");
    }

    [Fact]
    public void SetFirstName_Should_Accept_Name_At_Max_Length()
    {
        var owner = new Owner();
        var name = new string('A', Owner.FirstNameMaxLength);
        Should.NotThrow(() => owner.SetFirstName(name));
    }

    [Fact]
    public void SetFirstName_Should_Throw_When_Null()
    {
        var owner = new Owner();
        Should.Throw<DomainException>(() => owner.SetFirstName(null!));
    }

    [Fact]
    public void SetFirstName_Should_Throw_When_Empty()
    {
        var owner = new Owner();
        Should.Throw<DomainException>(() => owner.SetFirstName(string.Empty));
    }

    [Fact]
    public void SetFirstName_Should_Throw_When_Whitespace()
    {
        var owner = new Owner();
        Should.Throw<DomainException>(() => owner.SetFirstName("   "));
    }

    [Fact]
    public void SetFirstName_Should_Throw_When_Exceeds_Max_Length()
    {
        var owner = new Owner();
        var name = new string('A', Owner.FirstNameMaxLength + 1);
        Should.Throw<DomainException>(() => owner.SetFirstName(name));
    }

    // SetLastName

    [Fact]
    public void SetLastName_Should_Set_Name_When_Valid()
    {
        var owner = new Owner();
        owner.SetLastName("Smith");
        owner.LastName.ShouldBe("Smith");
    }

    [Fact]
    public void SetLastName_Should_Trim_Whitespace()
    {
        var owner = new Owner();
        owner.SetLastName("  Smith  ");
        owner.LastName.ShouldBe("Smith");
    }

    [Fact]
    public void SetLastName_Should_Accept_Name_At_Max_Length()
    {
        var owner = new Owner();
        var name = new string('A', Owner.LastNameMaxLength);
        Should.NotThrow(() => owner.SetLastName(name));
    }

    [Fact]
    public void SetLastName_Should_Throw_When_Null()
    {
        var owner = new Owner();
        Should.Throw<DomainException>(() => owner.SetLastName(null!));
    }

    [Fact]
    public void SetLastName_Should_Throw_When_Empty()
    {
        var owner = new Owner();
        Should.Throw<DomainException>(() => owner.SetLastName(string.Empty));
    }

    [Fact]
    public void SetLastName_Should_Throw_When_Whitespace()
    {
        var owner = new Owner();
        Should.Throw<DomainException>(() => owner.SetLastName("   "));
    }

    [Fact]
    public void SetLastName_Should_Throw_When_Exceeds_Max_Length()
    {
        var owner = new Owner();
        var name = new string('A', Owner.LastNameMaxLength + 1);
        Should.Throw<DomainException>(() => owner.SetLastName(name));
    }

    // SetEmail

    [Fact]
    public void SetEmail_Should_Set_Email_When_Valid()
    {
        var owner = new Owner();
        owner.SetEmail("alice@example.com");
        owner.Email.ShouldBe("alice@example.com");
    }

    [Fact]
    public void SetEmail_Should_Lowercase_And_Trim()
    {
        var owner = new Owner();
        owner.SetEmail("  ALICE@EXAMPLE.COM  ");
        owner.Email.ShouldBe("alice@example.com");
    }

    [Fact]
    public void SetEmail_Should_Accept_Email_At_Max_Length()
    {
        var owner = new Owner();
        var localPart = new string('a', Owner.EmailMaxLength - "@b.co".Length);
        var email = $"{localPart}@b.co";
        Should.NotThrow(() => owner.SetEmail(email));
    }

    [Fact]
    public void SetEmail_Should_Throw_When_Null()
    {
        var owner = new Owner();
        Should.Throw<DomainException>(() => owner.SetEmail(null!));
    }

    [Fact]
    public void SetEmail_Should_Throw_When_Empty()
    {
        var owner = new Owner();
        Should.Throw<DomainException>(() => owner.SetEmail(string.Empty));
    }

    [Fact]
    public void SetEmail_Should_Throw_When_Whitespace()
    {
        var owner = new Owner();
        Should.Throw<DomainException>(() => owner.SetEmail("   "));
    }

    [Fact]
    public void SetEmail_Should_Throw_When_Missing_At_Symbol()
    {
        var owner = new Owner();
        Should.Throw<DomainException>(() => owner.SetEmail("aliceexample.com"));
    }

    [Fact]
    public void SetEmail_Should_Throw_When_At_Symbol_At_Start()
    {
        var owner = new Owner();
        Should.Throw<DomainException>(() => owner.SetEmail("@example.com"));
    }

    [Fact]
    public void SetEmail_Should_Throw_When_No_Domain()
    {
        var owner = new Owner();
        Should.Throw<DomainException>(() => owner.SetEmail("alice@"));
    }

    [Fact]
    public void SetEmail_Should_Throw_When_No_TLD()
    {
        var owner = new Owner();
        Should.Throw<DomainException>(() => owner.SetEmail("alice@example"));
    }

    [Fact]
    public void SetEmail_Should_Throw_When_Contains_Spaces()
    {
        var owner = new Owner();
        Should.Throw<DomainException>(() => owner.SetEmail("alice smith@example.com"));
    }

    [Fact]
    public void SetEmail_Should_Throw_When_Exceeds_Max_Length()
    {
        var owner = new Owner();
        var localPart = new string('a', Owner.EmailMaxLength - "@b.co".Length + 1);
        var email = $"{localPart}@b.co";
        Should.Throw<DomainException>(() => owner.SetEmail(email));
    }

    // SetProfileImage / RemoveProfileImage

    [Fact]
    public void SetProfileImage_Should_Set_Image_When_Valid()
    {
        var owner = new Owner();
        owner.SetProfileImage("images/avatar.jpg", "image/jpeg");
        owner.ProfileImage.ShouldNotBeNull();
    }

    [Fact]
    public void SetProfileImage_Should_Throw_When_BlobName_Is_Null()
    {
        var owner = new Owner();
        Should.Throw<DomainException>(() => owner.SetProfileImage(null!, "image/jpeg"));
    }

    [Fact]
    public void SetProfileImage_Should_Throw_When_BlobName_Is_Empty()
    {
        var owner = new Owner();
        Should.Throw<DomainException>(() => owner.SetProfileImage(string.Empty, "image/jpeg"));
    }

    [Fact]
    public void SetProfileImage_Should_Throw_When_ContentType_Is_Null()
    {
        var owner = new Owner();
        Should.Throw<DomainException>(() => owner.SetProfileImage("images/avatar.jpg", null!));
    }

    [Fact]
    public void SetProfileImage_Should_Throw_When_ContentType_Is_Empty()
    {
        var owner = new Owner();
        Should.Throw<DomainException>(() => owner.SetProfileImage("images/avatar.jpg", string.Empty));
    }

    [Fact]
    public void SetProfileImage_Should_Throw_When_ContentType_Is_Not_Supported()
    {
        var owner = new Owner();
        Should.Throw<DomainException>(() => owner.SetProfileImage("images/avatar.jpg", "image/webp"));
    }

    [Fact]
    public void RemoveProfileImage_Should_Clear_Profile_Image()
    {
        var owner = new Owner();
        owner.SetProfileImage("images/avatar.jpg", "image/jpeg");

        owner.RemoveProfileImage();

        owner.ProfileImage.ShouldBeNull();
    }
}
