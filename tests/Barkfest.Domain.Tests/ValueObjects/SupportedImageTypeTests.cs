using Barkfest.Domain.ValueObjects;

namespace Barkfest.Domain.Tests.ValueObjects;

public class SupportedImageTypeTests
{
    // -----------------------------------------------------------------------
    // IsAllowedContentType
    // -----------------------------------------------------------------------

    [Theory]
    [InlineData("image/jpeg")]
    [InlineData("image/jpg")]
    [InlineData("image/png")]
    public void IsAllowedContentType_AllowedTypes_ReturnsTrue(string contentType)
    {
        SupportedImageType.IsAllowedContentType(contentType).ShouldBeTrue();
    }

    [Theory]
    [InlineData("IMAGE/JPEG")]
    [InlineData("Image/Png")]
    [InlineData("image/JPG")]
    public void IsAllowedContentType_AllowedTypesUppercase_ReturnsTrueAfterNormalisation(string contentType)
    {
        SupportedImageType.IsAllowedContentType(contentType).ShouldBeTrue();
    }

    [Theory]
    [InlineData("image/gif")]
    [InlineData("image/webp")]
    [InlineData("image/bmp")]
    [InlineData("application/octet-stream")]
    [InlineData("")]
    public void IsAllowedContentType_DisallowedTypes_ReturnsFalse(string contentType)
    {
        SupportedImageType.IsAllowedContentType(contentType).ShouldBeFalse();
    }

    // -----------------------------------------------------------------------
    // IsAllowedExtension
    // -----------------------------------------------------------------------

    [Theory]
    [InlineData("photo.jpg")]
    [InlineData("photo.jpeg")]
    [InlineData("photo.png")]
    public void IsAllowedExtension_AllowedFileNames_ReturnsTrue(string fileName)
    {
        SupportedImageType.IsAllowedExtension(fileName).ShouldBeTrue();
    }

    [Theory]
    [InlineData("photo.JPG")]
    [InlineData("photo.JPEG")]
    [InlineData("photo.PNG")]
    public void IsAllowedExtension_AllowedFileNamesUppercase_ReturnsTrueAfterNormalisation(string fileName)
    {
        SupportedImageType.IsAllowedExtension(fileName).ShouldBeTrue();
    }

    [Theory]
    [InlineData("photo.gif")]
    [InlineData("photo.webp")]
    [InlineData("photo.bmp")]
    [InlineData("photo.tiff")]
    [InlineData("photo")]
    public void IsAllowedExtension_DisallowedFileNames_ReturnsFalse(string fileName)
    {
        SupportedImageType.IsAllowedExtension(fileName).ShouldBeFalse();
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
