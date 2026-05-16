using Barkfest.Domain.Entities;
using Barkfest.Domain.Exceptions;

namespace Barkfest.Domain.Tests.Entities;

public class PetImageTests
{
    // -----------------------------------------------------------------------
    // SetImage
    // -----------------------------------------------------------------------

    [Fact]
    public void SetImage_ValidArgs_SetsBlobNameAndNormalisedContentType()
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
    public void SetImage_AllowedContentTypes_Succeeds(string contentType)
    {
        var image = new PetImage();

        Should.NotThrow(() => image.SetImage("pets/abc/photo", contentType));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void SetImage_EmptyBlobName_ThrowsDomainException(string blobName)
    {
        var image = new PetImage();

        Should.Throw<DomainException>(() => image.SetImage(blobName, "image/jpeg"))
            .Message.ShouldBe("Blob name is required.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void SetImage_EmptyContentType_ThrowsDomainException(string contentType)
    {
        var image = new PetImage();

        Should.Throw<DomainException>(() => image.SetImage("pets/abc/photo.jpg", contentType))
            .Message.ShouldBe("Content type is required.");
    }

    [Fact]
    public void SetImage_UnsupportedContentType_ThrowsDomainException()
    {
        var image = new PetImage();

        Should.Throw<DomainException>(() => image.SetImage("pets/abc/photo.gif", "image/gif"))
            .Message.ShouldContain("image/gif");
    }

    // -----------------------------------------------------------------------
    // SetDisplayOrder
    // -----------------------------------------------------------------------

    [Fact]
    public void SetDisplayOrder_Zero_SetsValue()
    {
        var image = new PetImage();

        image.SetDisplayOrder(0);

        image.DisplayOrder.ShouldBe(0);
    }

    [Fact]
    public void SetDisplayOrder_PositiveValue_SetsValue()
    {
        var image = new PetImage();

        image.SetDisplayOrder(3);

        image.DisplayOrder.ShouldBe(3);
    }

    [Fact]
    public void SetDisplayOrder_NegativeValue_ThrowsDomainException()
    {
        var image = new PetImage();

        Should.Throw<DomainException>(() => image.SetDisplayOrder(-1))
            .Message.ShouldBe("Display order must be zero or greater.");
    }

    // -----------------------------------------------------------------------
    // Id default
    // -----------------------------------------------------------------------

    [Fact]
    public void NewPetImage_HasNonEmptyId()
    {
        var image = new PetImage();

        image.Id.ShouldNotBe(Guid.Empty);
    }
}
