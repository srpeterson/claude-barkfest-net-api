using Barkfest.Domain.Exceptions;
using Barkfest.Domain.ValueObjects;

namespace Barkfest.Domain.Tests.ValueObjects;

public class ProfileImageTests
{
    // -----------------------------------------------------------------------
    // Create — happy path
    // -----------------------------------------------------------------------

    [Fact]
    public void Create_ValidArgs_ReturnProfileImageWithNormalisedValues()
    {
        var image = ProfileImage.Create("owners/abc/profile.jpg", "image/JPEG");

        image.BlobName.ShouldBe("owners/abc/profile.jpg");
        image.ContentType.ShouldBe("image/jpeg");
    }

    [Theory]
    [InlineData("image/jpeg")]
    [InlineData("image/jpg")]
    [InlineData("image/png")]
    public void Create_AllAllowedContentTypes_Succeeds(string contentType)
    {
        Should.NotThrow(() => ProfileImage.Create("owners/abc/photo", contentType));
    }

    // -----------------------------------------------------------------------
    // Create — validation failures
    // -----------------------------------------------------------------------

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_EmptyBlobName_ThrowsDomainException(string blobName)
    {
        Should.Throw<DomainException>(() => ProfileImage.Create(blobName, "image/jpeg"))
            .Message.ShouldBe("Blob name is required.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_EmptyContentType_ThrowsDomainException(string contentType)
    {
        Should.Throw<DomainException>(() => ProfileImage.Create("owners/abc/photo.jpg", contentType))
            .Message.ShouldBe("Content type is required.");
    }

    [Fact]
    public void Create_UnsupportedContentType_ThrowsDomainException()
    {
        Should.Throw<DomainException>(() => ProfileImage.Create("owners/abc/photo.gif", "image/gif"))
            .Message.ShouldContain("image/gif");
    }

    // -----------------------------------------------------------------------
    // Record equality
    // -----------------------------------------------------------------------

    [Fact]
    public void TwoProfileImages_SameBlobNameAndContentType_AreEqual()
    {
        var a = ProfileImage.Create("owners/abc/profile.jpg", "image/jpeg");
        var b = ProfileImage.Create("owners/abc/profile.jpg", "image/jpeg");

        a.ShouldBe(b);
    }

    [Fact]
    public void TwoProfileImages_DifferentBlobName_AreNotEqual()
    {
        var a = ProfileImage.Create("owners/abc/profile.jpg", "image/jpeg");
        var b = ProfileImage.Create("owners/xyz/profile.jpg", "image/jpeg");

        a.ShouldNotBe(b);
    }

    [Fact]
    public void TwoProfileImages_DifferentContentType_AreNotEqual()
    {
        var a = ProfileImage.Create("owners/abc/profile.jpg", "image/jpeg");
        var b = ProfileImage.Create("owners/abc/profile.png", "image/png");

        a.ShouldNotBe(b);
    }

    // -----------------------------------------------------------------------
    // BlobName trimming
    // -----------------------------------------------------------------------

    [Fact]
    public void Create_BlobNameWithWhitespace_IsTrimmed()
    {
        var image = ProfileImage.Create("  owners/abc/profile.jpg  ", "image/jpeg");

        image.BlobName.ShouldBe("owners/abc/profile.jpg");
    }
}
