using Barkfest.Domain.Exceptions;
using Barkfest.Domain.ValueObjects;

namespace Barkfest.Domain.Tests;

public class ProfileImageTests
{
    [Fact]
    public void Create_Should_Return_ProfileImage_When_Valid()
    {
        var result = ProfileImage.Create("images/photo.jpg", "image/jpeg");

        result.ShouldNotBeNull();
        result.BlobName.ShouldBe("images/photo.jpg");
        result.ContentType.ShouldBe("image/jpeg");
    }

    [Fact]
    public void Create_Should_Trim_BlobName()
    {
        var result = ProfileImage.Create("  images/photo.jpg  ", "image/jpeg");

        result.BlobName.ShouldBe("images/photo.jpg");
    }

    [Fact]
    public void Create_Should_Lowercase_And_Trim_ContentType()
    {
        var result = ProfileImage.Create("images/photo.jpg", "  IMAGE/JPEG  ");

        result.ContentType.ShouldBe("image/jpeg");
    }

    [Theory]
    [InlineData("image/jpeg")]
    [InlineData("image/jpg")]
    [InlineData("image/png")]
    public void Create_Should_Accept_Supported_Content_Types(string contentType)
    {
        var result = ProfileImage.Create("images/photo", contentType);

        result.ShouldNotBeNull();
    }

    [Fact]
    public void Create_Should_Throw_When_BlobName_Is_Null()
    {
        Should.Throw<DomainException>(() => ProfileImage.Create(null!, "image/jpeg"));
    }

    [Fact]
    public void Create_Should_Throw_When_BlobName_Is_Empty()
    {
        Should.Throw<DomainException>(() => ProfileImage.Create(string.Empty, "image/jpeg"));
    }

    [Fact]
    public void Create_Should_Throw_When_BlobName_Is_Whitespace()
    {
        Should.Throw<DomainException>(() => ProfileImage.Create("   ", "image/jpeg"));
    }

    [Fact]
    public void Create_Should_Throw_When_ContentType_Is_Null()
    {
        Should.Throw<DomainException>(() => ProfileImage.Create("images/photo.jpg", null!));
    }

    [Fact]
    public void Create_Should_Throw_When_ContentType_Is_Empty()
    {
        Should.Throw<DomainException>(() => ProfileImage.Create("images/photo.jpg", string.Empty));
    }

    [Fact]
    public void Create_Should_Throw_When_ContentType_Is_Whitespace()
    {
        Should.Throw<DomainException>(() => ProfileImage.Create("images/photo.jpg", "   "));
    }

    [Fact]
    public void Create_Should_Throw_When_ContentType_Is_Not_Supported()
    {
        Should.Throw<DomainException>(() => ProfileImage.Create("images/photo.jpg", "image/webp"));
    }

    [Fact]
    public void ProfileImages_With_Same_Values_Should_Be_Equal()
    {
        var a = ProfileImage.Create("images/photo.jpg", "image/jpeg");
        var b = ProfileImage.Create("images/photo.jpg", "image/jpeg");

        a.ShouldBe(b);
    }

    [Fact]
    public void ProfileImages_With_Different_Values_Should_Not_Be_Equal()
    {
        var a = ProfileImage.Create("images/photo.jpg", "image/jpeg");
        var b = ProfileImage.Create("images/other.png", "image/png");

        a.ShouldNotBe(b);
    }
}
