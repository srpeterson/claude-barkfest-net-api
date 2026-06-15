using Barkfest.Domain.Entities;
using Barkfest.Domain.Exceptions;
using Barkfest.Domain.ValueObjects;

namespace Barkfest.Domain.Tests.Entities;

public class OwnerTests
{
    // -----------------------------------------------------------------------
    // SetDisplayName
    // -----------------------------------------------------------------------

    [Fact]
    public void SetDisplayName_When_Valid_Sets_TrimmedDisplayName()
    {
        var owner = new Owner();

        owner.SetDisplayName("  FurParent  ");

        owner.DisplayName.ShouldBe("FurParent");
    }

    [Fact]
    public void SetDisplayName_When_AtMaxLength_Sets_DisplayName()
    {
        var owner = new Owner();
        var name = new string('x', Owner.DisplayNameMaxLength);

        owner.SetDisplayName(name);

        owner.DisplayName.ShouldBe(name);
    }

    [Fact]
    public void SetDisplayName_When_Null_Clears_DisplayName()
    {
        var owner = new Owner();
        owner.SetDisplayName("FurParent");

        owner.SetDisplayName(null);

        owner.DisplayName.ShouldBeNull();
    }

    [Fact]
    public void SetDisplayName_When_Whitespace_Clears_DisplayName()
    {
        var owner = new Owner();
        owner.SetDisplayName("FurParent");

        owner.SetDisplayName("   ");

        owner.DisplayName.ShouldBeNull();
    }

    [Fact]
    public void SetDisplayName_When_ExceedsMaxLength_Throws_DomainException()
    {
        var owner = new Owner();
        var longName = new string('x', Owner.DisplayNameMaxLength + 1);

        Should.Throw<DomainException>(() => owner.SetDisplayName(longName))
            .Message.ShouldContain(Owner.DisplayNameMaxLength.ToString());
    }

    [Theory]
    [InlineData("Cool Pet Dad")]
    [InlineData("cool pet dad")]
    [InlineData("COOLPETDAD")]
    public void SetDisplayName_When_Provided_Sets_NormalizedFormStrippedAndLowercased(string input)
    {
        var owner = new Owner();

        owner.SetDisplayName(input);

        owner.DisplayNameNormalized.ShouldBe("coolpetdad");
    }

    [Fact]
    public void SetDisplayName_When_Null_Clears_DisplayNameNormalized()
    {
        var owner = new Owner();
        owner.SetDisplayName("FurParent");

        owner.SetDisplayName(null);

        owner.DisplayNameNormalized.ShouldBeNull();
    }

    // -----------------------------------------------------------------------
    // SetUsername
    // -----------------------------------------------------------------------

    [Fact]
    public void SetUsername_When_UsernameIsValid_Sets_TrimmedUsername()
    {
        var owner = new Owner();

        owner.SetUsername("  JohnDoe  ");

        owner.Username.ShouldBe("JohnDoe");
    }

    [Fact]
    public void SetUsername_When_UsernamePreservesCase()
    {
        var owner = new Owner();

        owner.SetUsername("JohnDoe");

        owner.Username.ShouldBe("JohnDoe");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void SetUsername_When_EmptyOrWhitespace_Throws_DomainException(string username)
    {
        var owner = new Owner();

        Should.Throw<DomainException>(() => owner.SetUsername(username))
            .Message.ShouldBe("Username is required.");
    }

    [Fact]
    public void SetUsername_When_ExceedsMaxLength_Throws_DomainException()
    {
        var owner = new Owner();
        var longUsername = new string('a', AccountConstraints.UsernameMaxLength + 1);

        Should.Throw<DomainException>(() => owner.SetUsername(longUsername))
            .Message.ShouldContain(AccountConstraints.UsernameMaxLength.ToString());
    }

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
        var longLocal = new string('a', AccountConstraints.EmailMaxLength) + "@b.com";

        Should.Throw<DomainException>(() => owner.SetEmail(longLocal))
            .Message.ShouldContain(AccountConstraints.EmailMaxLength.ToString());
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

    [Theory]
    [InlineData("+15555550100")]
    [InlineData("+447911123456")]
    [InlineData("+33612345678")]
    public void SetPhoneNumber_When_ValidE164_Sets_PhoneNumber(string phoneNumber)
    {
        var owner = new Owner();

        owner.SetPhoneNumber(phoneNumber);

        owner.PhoneNumber.ShouldBe(phoneNumber);
    }

    [Fact]
    public void SetPhoneNumber_When_ValidE164WithWhitespace_Trims_AndSets()
    {
        var owner = new Owner();

        owner.SetPhoneNumber("  +15555550100  ");

        owner.PhoneNumber.ShouldBe("+15555550100");
    }

    [Fact]
    public void SetPhoneNumber_When_ExceedsMaxLength_Throws_DomainException()
    {
        var owner = new Owner();
        var longNumber = "+" + new string('1', E164PhoneNumber.MaxLength);

        var act = () => owner.SetPhoneNumber(longNumber);

        act.ShouldThrow<DomainException>();
    }

    [Theory]
    [InlineData("+1-555-555-0100")]
    [InlineData("5555550100")]
    [InlineData("+0123456789")]
    [InlineData("(555) 555-0100")]
    [InlineData("555.555.0100")]
    public void SetPhoneNumber_When_NotE164Format_Throws_DomainException(string phoneNumber)
    {
        var owner = new Owner();

        var act = () => owner.SetPhoneNumber(phoneNumber);

        act.ShouldThrow<DomainException>();
    }

    [Fact]
    public void SetPhoneNumber_When_Null_Sets_Null()
    {
        var owner = new Owner();
        owner.SetPhoneNumber("+15555550100");

        owner.SetPhoneNumber(null);

        owner.PhoneNumber.ShouldBeNull();
    }

    [Fact]
    public void SetPhoneNumber_When_Empty_Sets_Null()
    {
        var owner = new Owner();
        owner.SetPhoneNumber("+15555550100");

        owner.SetPhoneNumber("   ");

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
    // SetVerificationToken / MarkEmailVerified
    // -----------------------------------------------------------------------

    [Fact]
    public void NewOwner_When_Instantiated_Returns_IsEmailVerifiedFalse()
    {
        var owner = new Owner();

        owner.IsEmailVerified.ShouldBeFalse();
    }

    [Fact]
    public void SetVerificationToken_When_TokenIsValid_Sets_VerificationToken()
    {
        var owner = new Owner();

        owner.SetVerificationToken("abc123token");

        owner.VerificationToken.ShouldBe("abc123token");
    }

    [Fact]
    public void SetVerificationToken_When_TokenHasWhitespace_Trims_AndSets()
    {
        var owner = new Owner();

        owner.SetVerificationToken("  abc123token  ");

        owner.VerificationToken.ShouldBe("abc123token");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void SetVerificationToken_When_EmptyOrWhitespace_Throws_DomainException(string token)
    {
        var owner = new Owner();

        Should.Throw<DomainException>(() => owner.SetVerificationToken(token))
            .Message.ShouldBe("Verification token is required.");
    }

    [Fact]
    public void MarkEmailVerified_When_Called_Sets_IsEmailVerifiedTrue()
    {
        var owner = new Owner();
        owner.SetVerificationToken("abc123token");

        owner.MarkEmailVerified();

        owner.IsEmailVerified.ShouldBeTrue();
    }

    [Fact]
    public void MarkEmailVerified_When_Called_Clears_VerificationToken()
    {
        var owner = new Owner();
        owner.SetVerificationToken("abc123token");

        owner.MarkEmailVerified();

        owner.VerificationToken.ShouldBeNull();
    }

    // -----------------------------------------------------------------------
    // SetIsActive
    // -----------------------------------------------------------------------

    [Fact]
    public void SetIsActive_When_SetToFalse_Sets_IsActiveFalse()
    {
        var owner = new Owner();

        owner.SetIsActive(false);

        owner.IsActive.ShouldBeFalse();
    }

    [Fact]
    public void SetIsActive_When_SetToTrue_Sets_IsActiveTrue()
    {
        var owner = new Owner();
        owner.SetIsActive(false);

        owner.SetIsActive(true);

        owner.IsActive.ShouldBeTrue();
    }

    [Fact]
    public void NewOwner_When_Instantiated_Returns_IsActiveTrue()
    {
        var owner = new Owner();

        owner.IsActive.ShouldBeTrue();
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
