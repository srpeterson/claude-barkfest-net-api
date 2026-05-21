using Barkfest.Domain.Entities;
using Barkfest.Domain.Exceptions;

namespace Barkfest.Domain.Tests.Entities;

public class PetImageTests
{
    // -----------------------------------------------------------------------
    // SetImage
    // -----------------------------------------------------------------------

    [Fact]
    public void SetImage_When_ContentTypeHasMixedCase_Sets_NormalisedContentType()
    {
        var image = new PetImage();

        image.SetImage("pets/abc/photo.jpg", "image/JPEG");

        image.BlobName.ShouldBe("pets/abc/photo.jpg");
        image.ContentType.ShouldBe("image/jpeg");
    }

    [Theory]
    [InlineData("image/jpeg")]
    [InlineData("image/jpg")]
    [InlineData("image/png")]
    public void SetImage_When_ContentTypeIsAllowed_Succeeds(string contentType)
    {
        var image = new PetImage();

        Should.NotThrow(() => image.SetImage("pets/abc/photo", contentType));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void SetImage_When_BlobNameIsEmptyOrWhitespace_Throws_DomainException(string blobName)
    {
        var image = new PetImage();

        Should.Throw<DomainException>(() => image.SetImage(blobName, "image/jpeg"))
            .Message.ShouldBe("Blob name is required.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void SetImage_When_ContentTypeIsEmptyOrWhitespace_Throws_DomainException(string contentType)
    {
        var image = new PetImage();

        Should.Throw<DomainException>(() => image.SetImage("pets/abc/photo.jpg", contentType))
            .Message.ShouldBe("Content type is required.");
    }

    [Fact]
    public void SetImage_When_ContentTypeIsNotSupported_Throws_DomainException()
    {
        var image = new PetImage();

        Should.Throw<DomainException>(() => image.SetImage("pets/abc/photo.gif", "image/gif"))
            .Message.ShouldContain("image/gif");
    }

    // -----------------------------------------------------------------------
    // SetDisplayOrder
    // -----------------------------------------------------------------------

    [Fact]
    public void SetDisplayOrder_When_ValueIsZero_Sets_Value()
    {
        var image = new PetImage();

        image.SetDisplayOrder(0);

        image.DisplayOrder.ShouldBe(0);
    }

    [Fact]
    public void SetDisplayOrder_When_ValueIsPositive_Sets_Value()
    {
        var image = new PetImage();

        image.SetDisplayOrder(3);

        image.DisplayOrder.ShouldBe(3);
    }

    [Fact]
    public void SetDisplayOrder_When_ValueIsNegative_Throws_DomainException()
    {
        var image = new PetImage();

        Should.Throw<DomainException>(() => image.SetDisplayOrder(-1))
            .Message.ShouldBe("Display order must be zero or greater.");
    }

    // -----------------------------------------------------------------------
    // SetAsFeatured / UnsetAsFeatured
    // -----------------------------------------------------------------------

    [Fact]
    public void SetAsFeatured_When_Called_Sets_IsFeaturedImageTrue()
    {
        var image = new PetImage();

        image.SetAsFeatured();

        image.IsFeaturedImage.ShouldBeTrue();
    }

    [Fact]
    public void UnsetAsFeatured_When_Called_Sets_IsFeaturedImageFalse()
    {
        var image = new PetImage();
        image.SetAsFeatured();

        image.UnsetAsFeatured();

        image.IsFeaturedImage.ShouldBeFalse();
    }

    [Fact]
    public void NewPetImage_When_Instantiated_IsFeaturedImage_Is_False()
    {
        var image = new PetImage();

        image.IsFeaturedImage.ShouldBeFalse();
    }

    // -----------------------------------------------------------------------
    // Id default
    // -----------------------------------------------------------------------

    [Fact]
    public void NewPetImage_When_Instantiated_Returns_ValidGuid()
    {
        var image = new PetImage();

        image.Id.ShouldNotBe(Guid.Empty);
    }
}
