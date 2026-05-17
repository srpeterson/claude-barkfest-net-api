using Barkfest.Application.Features.Pets.Commands.AddPetImage;

namespace Barkfest.Application.Tests.Features.Pets.Commands;

public class AddPetImageCommandValidatorTests
{
    private readonly AddPetImageCommandValidator _sut = new();

    private static AddPetImageCommand ValidCommand(
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
    public void Validate_SupportedFileAndContentType_Passes(string fileName, string contentType)
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
    public void Validate_ContentTypeEmptyOrWhitespace_FailsOnContentType(string contentType)
    {
        var result = _sut.Validate(ValidCommand(contentType: contentType));

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(AddPetImageCommand.ContentType));
    }

    [Theory]
    [InlineData("image/webp")]
    [InlineData("image/gif")]
    [InlineData("application/octet-stream")]
    public void Validate_UnsupportedContentType_FailsOnContentType(string contentType)
    {
        var result = _sut.Validate(ValidCommand(contentType: contentType));

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(AddPetImageCommand.ContentType));
    }

    // -----------------------------------------------------------------------
    // FileName
    // -----------------------------------------------------------------------

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_FileNameEmptyOrWhitespace_FailsOnFileName(string fileName)
    {
        var result = _sut.Validate(ValidCommand(fileName: fileName));

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(AddPetImageCommand.FileName));
    }

    [Theory]
    [InlineData("photo.webp")]
    [InlineData("photo.gif")]
    [InlineData("photo.bmp")]
    [InlineData("photo")]
    public void Validate_UnsupportedFileExtension_FailsOnFileName(string fileName)
    {
        var result = _sut.Validate(ValidCommand(fileName: fileName));

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(AddPetImageCommand.FileName));
    }
}
