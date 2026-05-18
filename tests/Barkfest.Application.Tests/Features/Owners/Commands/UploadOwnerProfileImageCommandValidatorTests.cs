using Barkfest.Application.Features.Owners.Commands.UploadOwnerProfileImage;

namespace Barkfest.Application.Tests.Features.Owners.Commands;

public class UploadOwnerProfileImageCommandValidatorTests
{
    private readonly UploadOwnerProfileImageCommandValidator _sut = new();

    private static UploadOwnerProfileImageCommand ValidCommand(
        string fileName = "photo.jpg",
        string contentType = "image/jpeg") =>
        new(Guid.NewGuid(), fileName, Stream.Null, contentType);

    // -----------------------------------------------------------------------
    // Valid commands
    // -----------------------------------------------------------------------

    [Theory]
    [InlineData("photo.jpg", "image/jpeg")]
    [InlineData("photo.jpeg", "image/jpeg")]
    [InlineData("photo.png", "image/png")]
    [InlineData("photo.jpg", "image/jpg")]
    public void Validate_When_FileAndContentTypeAreSupported_Passes(string fileName, string contentType)
    {
        var result = _sut.Validate(ValidCommand(fileName, contentType));

        result.IsValid.ShouldBeTrue();
    }

    // -----------------------------------------------------------------------
    // ContentType
    // -----------------------------------------------------------------------

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_When_ContentTypeIsEmptyOrWhitespace_Fails_ForContentType(string contentType)
    {
        var result = _sut.Validate(ValidCommand(contentType: contentType));

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(UploadOwnerProfileImageCommand.ContentType));
    }

    [Theory]
    [InlineData("image/webp")]
    [InlineData("image/gif")]
    [InlineData("application/octet-stream")]
    public void Validate_When_ContentTypeIsNotSupported_Fails_ForContentType(string contentType)
    {
        var result = _sut.Validate(ValidCommand(contentType: contentType));

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(UploadOwnerProfileImageCommand.ContentType));
    }

    // -----------------------------------------------------------------------
    // FileName
    // -----------------------------------------------------------------------

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_When_FileNameIsEmptyOrWhitespace_Fails_ForFileName(string fileName)
    {
        var result = _sut.Validate(ValidCommand(fileName: fileName));

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(UploadOwnerProfileImageCommand.FileName));
    }

    [Theory]
    [InlineData("photo.webp")]
    [InlineData("photo.gif")]
    [InlineData("photo.bmp")]
    [InlineData("photo")]
    public void Validate_When_FileExtensionIsNotSupported_Fails_ForFileName(string fileName)
    {
        var result = _sut.Validate(ValidCommand(fileName: fileName));

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(UploadOwnerProfileImageCommand.FileName));
    }
}
