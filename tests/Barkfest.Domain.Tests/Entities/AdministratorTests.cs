using Barkfest.Domain.Entities;
using Barkfest.Domain.Exceptions;

namespace Barkfest.Domain.Tests.Entities;

public class AdministratorTests
{
    // -----------------------------------------------------------------------
    // SetEmail
    // -----------------------------------------------------------------------

    [Fact]
    public void SetEmail_When_EmailIsValid_Sets_NormalizedLowercaseEmail()
    {
        var administrator = new Administrator();

        administrator.SetEmail("  Admin@Barkfest.DEV  ");

        administrator.Email.ShouldBe("admin@barkfest.dev");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void SetEmail_When_EmptyOrWhitespace_Throws_DomainException(string email)
    {
        var administrator = new Administrator();

        Should.Throw<DomainException>(() => administrator.SetEmail(email))
            .Message.ShouldBe("Email is required.");
    }

    [Fact]
    public void SetEmail_When_ExceedsMaxLength_Throws_DomainException()
    {
        var administrator = new Administrator();
        var longLocal = new string('a', Administrator.EmailMaxLength) + "@b.com";

        Should.Throw<DomainException>(() => administrator.SetEmail(longLocal))
            .Message.ShouldContain(Administrator.EmailMaxLength.ToString());
    }

    [Fact]
    public void SetEmail_When_ContainsSpace_Throws_DomainException()
    {
        var administrator = new Administrator();

        Should.Throw<DomainException>(() => administrator.SetEmail("admin user@barkfest.dev"))
            .Message.ShouldBe("Email must be a valid email address.");
    }

    [Fact]
    public void SetEmail_When_MissingAtSign_Throws_DomainException()
    {
        var administrator = new Administrator();

        Should.Throw<DomainException>(() => administrator.SetEmail("adminbarkfest.dev"))
            .Message.ShouldBe("Email must be a valid email address.");
    }

    // -----------------------------------------------------------------------
    // SetPasswordHash
    // -----------------------------------------------------------------------

    [Fact]
    public void SetPasswordHash_When_HashIsValid_Sets_PasswordHash()
    {
        var administrator = new Administrator();

        administrator.SetPasswordHash("$2a$11$hashedvalue");

        administrator.PasswordHash.ShouldBe("$2a$11$hashedvalue");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void SetPasswordHash_When_EmptyOrWhitespace_Throws_DomainException(string hash)
    {
        var administrator = new Administrator();

        Should.Throw<DomainException>(() => administrator.SetPasswordHash(hash))
            .Message.ShouldBe("Password hash is required.");
    }

    // -----------------------------------------------------------------------
    // Id / CreatedAt defaults
    // -----------------------------------------------------------------------

    [Fact]
    public void NewAdministrator_When_Instantiated_Returns_NonEmptyId()
    {
        var administrator = new Administrator();

        administrator.Id.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public void NewAdministrator_When_Instantiated_Returns_RecentUtcCreatedAt()
    {
        var before = DateTime.UtcNow;

        var administrator = new Administrator();

        administrator.CreatedAt.ShouldBeGreaterThanOrEqualTo(before);
        administrator.CreatedAt.ShouldBeLessThanOrEqualTo(DateTime.UtcNow);
    }
}
