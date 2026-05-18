using Barkfest.Domain.Entities;
using Barkfest.Domain.Exceptions;

namespace Barkfest.Domain.Tests.Entities;

public class OwnerTests
{
    // -----------------------------------------------------------------------
    // SetFirstName
    // -----------------------------------------------------------------------

    [Fact]
    public void SetFirstName_When_NameIsValid_Sets_TrimmedFirstName()
    {
        var owner = new Owner();

        owner.SetFirstName("  Alice  ");

        owner.FirstName.ShouldBe("Alice");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void SetFirstName_When_EmptyOrWhitespace_Throws_DomainException(string name)
    {
        var owner = new Owner();

        Should.Throw<DomainException>(() => owner.SetFirstName(name))
            .Message.ShouldBe("First name is required.");
    }

    [Fact]
    public void SetFirstName_When_ExceedsMaxLength_Throws_DomainException()
    {
        var owner = new Owner();
        var longName = new string('A', Owner.FirstNameMaxLength + 1);

        Should.Throw<DomainException>(() => owner.SetFirstName(longName))
            .Message.ShouldContain(Owner.FirstNameMaxLength.ToString());
    }

    // -----------------------------------------------------------------------
    // SetLastName
    // -----------------------------------------------------------------------

    [Fact]
    public void SetLastName_When_NameIsValid_Sets_TrimmedLastName()
    {
        var owner = new Owner();

        owner.SetLastName("  Smith  ");

        owner.LastName.ShouldBe("Smith");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void SetLastName_When_EmptyOrWhitespace_Throws_DomainException(string name)
    {
        var owner = new Owner();

        Should.Throw<DomainException>(() => owner.SetLastName(name))
            .Message.ShouldBe("Last name is required.");
    }

    [Fact]
    public void SetLastName_When_ExceedsMaxLength_Throws_DomainException()
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
    public void SetEmail_When_EmailIsValid_Sets_NormalizedLowercaseEmail()
    {
        var owner = new Owner();

        owner.SetEmail("  Alice@Example.COM  ");

        owner.Email.ShouldBe("alice@example.com");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void SetEmail_When_EmptyOrWhitespace_Throws_DomainException(string email)
    {
        var owner = new Owner();

        Should.Throw<DomainException>(() => owner.SetEmail(email))
            .Message.ShouldBe("Email is required.");
    }

    [Fact]
    public void SetEmail_When_ExceedsMaxLength_Throws_DomainException()
    {
        var owner = new Owner();
        var longLocal = new string('a', Owner.EmailMaxLength) + "@b.com";

        Should.Throw<DomainException>(() => owner.SetEmail(longLocal))
            .Message.ShouldContain(Owner.EmailMaxLength.ToString());
    }

    [Fact]
    public void SetEmail_When_ContainsSpace_Throws_DomainException()
    {
        var owner = new Owner();

        Should.Throw<DomainException>(() => owner.SetEmail("alice smith@example.com"))
            .Message.ShouldBe("Email must be a valid email address.");
    }

    [Fact]
    public void SetEmail_When_MissingAtSign_Throws_DomainException()
    {
        var owner = new Owner();

        Should.Throw<DomainException>(() => owner.SetEmail("aliceexample.com"))
            .Message.ShouldBe("Email must be a valid email address.");
    }

    [Fact]
    public void SetEmail_When_EmailStartsWithAtSymbol_Throws_DomainException()
    {
        var owner = new Owner();

        Should.Throw<DomainException>(() => owner.SetEmail("@example.com"))
            .Message.ShouldBe("Email must be a valid email address.");
    }

    [Fact]
    public void SetEmail_When_MissingDomain_Throws_DomainException()
    {
        var owner = new Owner();

        Should.Throw<DomainException>(() => owner.SetEmail("alice@"))
            .Message.ShouldBe("Email must be a valid email address.");
    }

    [Fact]
    public void SetEmail_When_DomainHasNoDot_Throws_DomainException()
    {
        var owner = new Owner();

        Should.Throw<DomainException>(() => owner.SetEmail("alice@examplecom"))
            .Message.ShouldBe("Email must be a valid email address.");
    }

    [Fact]
    public void SetEmail_When_DomainEndsWithDot_Throws_DomainException()
    {
        var owner = new Owner();

        Should.Throw<DomainException>(() => owner.SetEmail("alice@example."))
            .Message.ShouldBe("Email must be a valid email address.");
    }

    // -----------------------------------------------------------------------
    // SetPhoneNumber
    // -----------------------------------------------------------------------

    [Fact]
    public void SetPhoneNumber_When_NumberIsValid_Sets_TrimmedPhoneNumber()
    {
        var owner = new Owner();

        owner.SetPhoneNumber("  +1-555-0100  ");

        owner.PhoneNumber.ShouldBe("+1-555-0100");
    }

    [Fact]
    public void SetPhoneNumber_When_Null_Sets_Null()
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
    public void SetProfileImage_When_ArgsAreValid_Sets_ProfileImage()
    {
        var owner = new Owner();

        owner.SetProfileImage("owners/abc/profile.jpg", "image/jpeg");

        owner.ProfileImage.ShouldNotBeNull();
        owner.ProfileImage!.BlobName.ShouldBe("owners/abc/profile.jpg");
        owner.ProfileImage.ContentType.ShouldBe("image/jpeg");
    }

    [Fact]
    public void RemoveProfileImage_When_ImageIsSet_Clears_ProfileImage()
    {
        var owner = new Owner();
        owner.SetProfileImage("owners/abc/profile.jpg", "image/jpeg");

        owner.RemoveProfileImage();

        owner.ProfileImage.ShouldBeNull();
    }

    // -----------------------------------------------------------------------
    // SetActive
    // -----------------------------------------------------------------------

    [Fact]
    public void SetActive_When_SetToFalse_Sets_ActiveFalse()
    {
        var owner = new Owner();

        owner.SetActive(false);

        owner.Active.ShouldBeFalse();
    }

    [Fact]
    public void SetActive_When_SetToTrue_Sets_ActiveTrue()
    {
        var owner = new Owner();
        owner.SetActive(false);

        owner.SetActive(true);

        owner.Active.ShouldBeTrue();
    }

    [Fact]
    public void NewOwner_When_Instantiated_Returns_ActiveTrue()
    {
        var owner = new Owner();

        owner.Active.ShouldBeTrue();
    }

    // -----------------------------------------------------------------------
    // SetIsVisible
    // -----------------------------------------------------------------------

    [Fact]
    public void SetIsVisible_When_SetToFalse_Sets_IsVisibleFalse()
    {
        var owner = new Owner();

        owner.SetIsVisible(false);

        owner.IsVisible.ShouldBeFalse();
    }

    [Fact]
    public void SetIsVisible_When_SetToTrue_Sets_IsVisibleTrue()
    {
        var owner = new Owner();
        owner.SetIsVisible(false);

        owner.SetIsVisible(true);

        owner.IsVisible.ShouldBeTrue();
    }

    [Fact]
    public void NewOwner_When_Instantiated_Returns_IsVisibleTrue()
    {
        var owner = new Owner();

        owner.IsVisible.ShouldBeTrue();
    }

    // -----------------------------------------------------------------------
    // Id / CreatedAt defaults
    // -----------------------------------------------------------------------

    [Fact]
    public void NewOwner_When_Instantiated_Returns_NonEmptyId()
    {
        var owner = new Owner();

        owner.Id.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public void NewOwner_When_Instantiated_Returns_RecentUtcCreatedAt()
    {
        var before = DateTime.UtcNow;

        var owner = new Owner();

        owner.CreatedAt.ShouldBeGreaterThanOrEqualTo(before);
        owner.CreatedAt.ShouldBeLessThanOrEqualTo(DateTime.UtcNow);
    }
}
