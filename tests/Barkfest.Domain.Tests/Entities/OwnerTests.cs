using Barkfest.Domain.Entities;
using Barkfest.Domain.Exceptions;

namespace Barkfest.Domain.Tests.Entities;

public class OwnerTests
{
    // -----------------------------------------------------------------------
    // SetFirstName
    // -----------------------------------------------------------------------

    [Fact]
    public void SetFirstName_ValidName_SetsTrimmmedValue()
    {
        var owner = new Owner();

        owner.SetFirstName("  Alice  ");

        owner.FirstName.ShouldBe("Alice");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void SetFirstName_EmptyOrWhitespace_ThrowsDomainException(string name)
    {
        var owner = new Owner();

        Should.Throw<DomainException>(() => owner.SetFirstName(name))
            .Message.ShouldBe("First name is required.");
    }

    [Fact]
    public void SetFirstName_ExceedsMaxLength_ThrowsDomainException()
    {
        var owner = new Owner();
        var longName = new string('A', Owner.FirstNameMaxLength + 1);

        Should.Throw<DomainException>(() => owner.SetFirstName(longName))
            .Message.ShouldContain(Owner.FirstNameMaxLength.ToString());
    }

    [Fact]
    public void SetFirstName_ExactlyMaxLength_Succeeds()
    {
        var owner = new Owner();
        var name = new string('A', Owner.FirstNameMaxLength);

        owner.SetFirstName(name);

        owner.FirstName.ShouldBe(name);
    }

    // -----------------------------------------------------------------------
    // SetLastName
    // -----------------------------------------------------------------------

    [Fact]
    public void SetLastName_ValidName_SetsTrimmmedValue()
    {
        var owner = new Owner();

        owner.SetLastName("  Smith  ");

        owner.LastName.ShouldBe("Smith");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void SetLastName_EmptyOrWhitespace_ThrowsDomainException(string name)
    {
        var owner = new Owner();

        Should.Throw<DomainException>(() => owner.SetLastName(name))
            .Message.ShouldBe("Last name is required.");
    }

    [Fact]
    public void SetLastName_ExceedsMaxLength_ThrowsDomainException()
    {
        var owner = new Owner();
        var longName = new string('B', Owner.LastNameMaxLength + 1);

        Should.Throw<DomainException>(() => owner.SetLastName(longName))
            .Message.ShouldContain(Owner.LastNameMaxLength.ToString());
    }

    // -----------------------------------------------------------------------
    // SetEmail
    // -----------------------------------------------------------------------

    [Fact]
    public void SetEmail_ValidEmail_NormalizesToLowercase()
    {
        var owner = new Owner();

        owner.SetEmail("  Alice@Example.COM  ");

        owner.Email.ShouldBe("alice@example.com");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void SetEmail_EmptyOrWhitespace_ThrowsDomainException(string email)
    {
        var owner = new Owner();

        Should.Throw<DomainException>(() => owner.SetEmail(email))
            .Message.ShouldBe("Email is required.");
    }

    [Fact]
    public void SetEmail_ExceedsMaxLength_ThrowsDomainException()
    {
        var owner = new Owner();
        var longLocal = new string('a', Owner.EmailMaxLength) + "@b.com";

        Should.Throw<DomainException>(() => owner.SetEmail(longLocal))
            .Message.ShouldContain(Owner.EmailMaxLength.ToString());
    }

    [Fact]
    public void SetEmail_ContainsSpace_ThrowsDomainException()
    {
        var owner = new Owner();

        Should.Throw<DomainException>(() => owner.SetEmail("alice smith@example.com"))
            .Message.ShouldBe("Email must be a valid email address.");
    }

    [Fact]
    public void SetEmail_MissingAtSign_ThrowsDomainException()
    {
        var owner = new Owner();

        Should.Throw<DomainException>(() => owner.SetEmail("aliceexample.com"))
            .Message.ShouldBe("Email must be a valid email address.");
    }

    [Fact]
    public void SetEmail_AtSignAtStart_ThrowsDomainException()
    {
        var owner = new Owner();

        Should.Throw<DomainException>(() => owner.SetEmail("@example.com"))
            .Message.ShouldBe("Email must be a valid email address.");
    }

    [Fact]
    public void SetEmail_MissingDomain_ThrowsDomainException()
    {
        var owner = new Owner();

        Should.Throw<DomainException>(() => owner.SetEmail("alice@"))
            .Message.ShouldBe("Email must be a valid email address.");
    }

    [Fact]
    public void SetEmail_MissingDotInDomain_ThrowsDomainException()
    {
        var owner = new Owner();

        Should.Throw<DomainException>(() => owner.SetEmail("alice@examplecom"))
            .Message.ShouldBe("Email must be a valid email address.");
    }

    [Fact]
    public void SetEmail_DomainEndsWithDot_ThrowsDomainException()
    {
        var owner = new Owner();

        Should.Throw<DomainException>(() => owner.SetEmail("alice@example."))
            .Message.ShouldBe("Email must be a valid email address.");
    }

    // -----------------------------------------------------------------------
    // SetPhoneNumber
    // -----------------------------------------------------------------------

    [Fact]
    public void SetPhoneNumber_ValidNumber_SetsTrimmmedValue()
    {
        var owner = new Owner();

        owner.SetPhoneNumber("  +1-555-0100  ");

        owner.PhoneNumber.ShouldBe("+1-555-0100");
    }

    [Fact]
    public void SetPhoneNumber_Null_SetsNull()
    {
        var owner = new Owner();
        owner.SetPhoneNumber("555-0100");

        owner.SetPhoneNumber(null);

        owner.PhoneNumber.ShouldBeNull();
    }

    // -----------------------------------------------------------------------
    // SetProfileImage / RemoveProfileImage
    // -----------------------------------------------------------------------

    [Fact]
    public void SetProfileImage_ValidArgs_SetsProfileImage()
    {
        var owner = new Owner();

        owner.SetProfileImage("owners/abc/profile.jpg", "image/jpeg");

        owner.ProfileImage.ShouldNotBeNull();
        owner.ProfileImage!.BlobName.ShouldBe("owners/abc/profile.jpg");
        owner.ProfileImage.ContentType.ShouldBe("image/jpeg");
    }

    [Fact]
    public void RemoveProfileImage_WhenImageSet_ClearsProfileImage()
    {
        var owner = new Owner();
        owner.SetProfileImage("owners/abc/profile.jpg", "image/jpeg");

        owner.RemoveProfileImage();

        owner.ProfileImage.ShouldBeNull();
    }

    // -----------------------------------------------------------------------
    // Id / CreatedAt defaults
    // -----------------------------------------------------------------------

    [Fact]
    public void NewOwner_HasNonEmptyId()
    {
        var owner = new Owner();

        owner.Id.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public void NewOwner_CreatedAtIsRecentUtc()
    {
        var before = DateTime.UtcNow;

        var owner = new Owner();

        owner.CreatedAt.ShouldBeGreaterThanOrEqualTo(before);
        owner.CreatedAt.ShouldBeLessThanOrEqualTo(DateTime.UtcNow);
    }
}
