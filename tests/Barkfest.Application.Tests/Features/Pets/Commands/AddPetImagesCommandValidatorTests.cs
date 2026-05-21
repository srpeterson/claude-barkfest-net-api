using Barkfest.Application.Features.Pets.Commands.AddPetImages;

namespace Barkfest.Application.Tests.Features.Pets.Commands;

public class AddPetImagesCommandValidatorTests
{
    private readonly AddPetImagesCommandValidator _addPetImagesCommandValidator = new();

    private static AddPetImagesCommand ValidCommand(params (string FileName, string ContentType)[] files) =>
        new(Guid.NewGuid(), files
            .Select(f => new PetImageUpload(f.FileName, Stream.Null, f.ContentType))
            .ToList());

    // -----------------------------------------------------------------------
    // Valid commands
    // -----------------------------------------------------------------------

    [Theory]
    [InlineData("photo.jpg", "image/jpeg")]
    [InlineData("photo.jpeg", "image/jpeg")]
    [InlineData("photo.png", "image/png")]
    [InlineData("photo.jpg", "image/jpg")]
    public void Validate_When_SingleValidFile_Passes(string fileName, string contentType)
    {
        var result = _addPetImagesCommandValidator.Validate(ValidCommand((fileName, contentType)));

        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_When_MultipleValidFiles_Passes()
    {
        var result = _addPetImagesCommandValidator.Validate(
            ValidCommand(("photo1.jpg", "image/jpeg"), ("photo2.png", "image/png")));

        result.IsValid.ShouldBeTrue();
    }

    // -----------------------------------------------------------------------
    // Images collection
    // -----------------------------------------------------------------------

    [Fact]
    public void Validate_When_ImagesIsEmpty_Fails_ForImages()
    {
        var command = new AddPetImagesCommand(Guid.NewGuid(), []);

        var result = _addPetImagesCommandValidator.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(AddPetImagesCommand.Images));
    }

    // -----------------------------------------------------------------------
    // ContentType (per image)
    // -----------------------------------------------------------------------

    [Theory]
    [InlineData("image/webp")]
    [InlineData("image/gif")]
    [InlineData("application/octet-stream")]
    public void Validate_When_ContentTypeIsNotSupported_Fails(string contentType)
    {
        var result = _addPetImagesCommandValidator.Validate(ValidCommand(("photo.jpg", contentType)));

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName.EndsWith("ContentType"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_When_ContentTypeIsEmptyOrWhitespace_Fails(string contentType)
    {
        var result = _addPetImagesCommandValidator.Validate(ValidCommand(("photo.jpg", contentType)));

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName.EndsWith("ContentType"));
    }

    // -----------------------------------------------------------------------
    // FileName (per image)
    // -----------------------------------------------------------------------

    [Theory]
    [InlineData("photo.webp")]
    [InlineData("photo.gif")]
    [InlineData("photo.bmp")]
    [InlineData("photo")]
    public void Validate_When_FileExtensionIsNotSupported_Fails(string fileName)
    {
        var result = _addPetImagesCommandValidator.Validate(ValidCommand((fileName, "image/jpeg")));

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName.EndsWith("FileName"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_When_FileNameIsEmptyOrWhitespace_Fails(string fileName)
    {
        var result = _addPetImagesCommandValidator.Validate(ValidCommand((fileName, "image/jpeg")));

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName.EndsWith("FileName"));
    }
}
