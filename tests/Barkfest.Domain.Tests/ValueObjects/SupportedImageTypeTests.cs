using Barkfest.Domain.ValueObjects;

namespace Barkfest.Domain.Tests.ValueObjects;

public class SupportedImageTypeTests
{
    // -----------------------------------------------------------------------
    // IsContentTypeSupported
    // -----------------------------------------------------------------------

    [Theory]
    [InlineData("image/jpeg")]
    [InlineData("image/jpg")]
    [InlineData("image/png")]
    public void IsContentTypeSupported_When_ContentTypeIsAllowed_Returns_True(string contentType)
    {
        SupportedImageType.IsContentTypeSupported(contentType).ShouldBeTrue();
    }

    [Theory]
    [InlineData("IMAGE/JPEG")]
    [InlineData("Image/Png")]
    [InlineData("image/JPG")]
    public void IsContentTypeSupported_When_ContentTypeIsAllowedButUppercase_Returns_True(string contentType)
    {
        SupportedImageType.IsContentTypeSupported(contentType).ShouldBeTrue();
    }

    [Theory]
    [InlineData("image/gif")]
    [InlineData("image/webp")]
    [InlineData("image/bmp")]
    [InlineData("application/octet-stream")]
    [InlineData("")]
    public void IsContentTypeSupported_When_ContentTypeIsNotAllowed_Returns_False(string contentType)
    {
        SupportedImageType.IsContentTypeSupported(contentType).ShouldBeFalse();
    }

    // -----------------------------------------------------------------------
    // IsFileExtensionSupported
    // -----------------------------------------------------------------------

    [Theory]
    [InlineData("photo.jpg")]
    [InlineData("photo.jpeg")]
    [InlineData("photo.png")]
    public void IsFileExtensionSupported_When_ExtensionIsAllowed_Returns_True(string fileName)
    {
        SupportedImageType.IsFileExtensionSupported(fileName).ShouldBeTrue();
    }

    [Theory]
    [InlineData("photo.JPG")]
    [InlineData("photo.JPEG")]
    [InlineData("photo.PNG")]
    public void IsFileExtensionSupported_When_ExtensionIsAllowedButUppercase_Returns_True(string fileName)
    {
        SupportedImageType.IsFileExtensionSupported(fileName).ShouldBeTrue();
    }

    [Theory]
    [InlineData("photo.gif")]
    [InlineData("photo.webp")]
    [InlineData("photo.bmp")]
    [InlineData("photo.tiff")]
    [InlineData("photo")]
    public void IsFileExtensionSupported_When_ExtensionIsNotAllowed_Returns_False(string fileName)
    {
        SupportedImageType.IsFileExtensionSupported(fileName).ShouldBeFalse();
    }

    // -----------------------------------------------------------------------
    // AllowedContentTypes / AllowedExtensions collections
    // -----------------------------------------------------------------------

    [Fact]
    public void AllowedContentTypes_ContainsExpectedValues()
    {
        SupportedImageType.AllowedContentTypes.ShouldContain("image/jpeg");
        SupportedImageType.AllowedContentTypes.ShouldContain("image/jpg");
        SupportedImageType.AllowedContentTypes.ShouldContain("image/png");
    }

    [Fact]
    public void AllowedExtensions_ContainsExpectedValues()
    {
        SupportedImageType.AllowedExtensions.ShouldContain(".jpeg");
        SupportedImageType.AllowedExtensions.ShouldContain(".jpg");
        SupportedImageType.AllowedExtensions.ShouldContain(".png");
    }
}
