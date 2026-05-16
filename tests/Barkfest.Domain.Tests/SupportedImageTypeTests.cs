using Barkfest.Domain.ValueObjects;

namespace Barkfest.Domain.Tests;

public class SupportedImageTypeTests
{
    [Theory]
    [InlineData("image/jpeg")]
    [InlineData("image/jpg")]
    [InlineData("image/png")]
    public void IsAllowedContentType_Should_Return_True_For_Supported_Types(string contentType)
    {
        SupportedImageType.IsAllowedContentType(contentType).ShouldBeTrue();
    }

    [Fact]
    public void IsAllowedContentType_Should_Return_False_For_Unsupported_Type()
    {
        SupportedImageType.IsAllowedContentType("image/webp").ShouldBeFalse();
    }

    [Fact]
    public void IsAllowedContentType_Should_Be_Case_Insensitive()
    {
        SupportedImageType.IsAllowedContentType("IMAGE/JPEG").ShouldBeTrue();
    }

    [Theory]
    [InlineData(".jpeg")]
    [InlineData(".jpg")]
    [InlineData(".png")]
    public void IsAllowedExtension_Should_Return_True_For_Supported_Extensions(string extension)
    {
        SupportedImageType.IsAllowedExtension(extension).ShouldBeTrue();
    }

    [Fact]
    public void IsAllowedExtension_Should_Return_False_For_Unsupported_Extension()
    {
        SupportedImageType.IsAllowedExtension(".webp").ShouldBeFalse();
    }

    [Fact]
    public void IsAllowedExtension_Should_Be_Case_Insensitive()
    {
        SupportedImageType.IsAllowedExtension(".JPG").ShouldBeTrue();
    }

    [Fact]
    public void IsAllowedExtension_Should_Extract_Extension_From_Filename()
    {
        SupportedImageType.IsAllowedExtension("photo.jpg").ShouldBeTrue();
    }
}
