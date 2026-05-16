using Barkfest.Domain.Entities;
using Barkfest.Domain.Exceptions;

namespace Barkfest.Domain.Tests;

public class PetImageTests
{
    [Fact]
    public void SetImage_Should_Set_BlobName_And_ContentType_When_Valid()
    {
        var image = new PetImage();

        image.SetImage("images/photo.jpg", "image/jpeg");

        image.BlobName.ShouldBe("images/photo.jpg");
        image.ContentType.ShouldBe("image/jpeg");
    }

    [Theory]
    [InlineData("image/jpeg")]
    [InlineData("image/jpg")]
    [InlineData("image/png")]
    public void SetImage_Should_Accept_Supported_Content_Types(string contentType)
    {
        var image = new PetImage();

        Should.NotThrow(() => image.SetImage("images/photo", contentType));
    }

    [Fact]
    public void SetImage_Should_Throw_When_BlobName_Is_Null()
    {
        var image = new PetImage();

        Should.Throw<DomainException>(() => image.SetImage(null!, "image/jpeg"));
    }

    [Fact]
    public void SetImage_Should_Throw_When_BlobName_Is_Empty()
    {
        var image = new PetImage();

        Should.Throw<DomainException>(() => image.SetImage(string.Empty, "image/jpeg"));
    }

    [Fact]
    public void SetImage_Should_Throw_When_ContentType_Is_Null()
    {
        var image = new PetImage();

        Should.Throw<DomainException>(() => image.SetImage("images/photo.jpg", null!));
    }

    [Fact]
    public void SetImage_Should_Throw_When_ContentType_Is_Empty()
    {
        var image = new PetImage();

        Should.Throw<DomainException>(() => image.SetImage("images/photo.jpg", string.Empty));
    }

    [Fact]
    public void SetImage_Should_Throw_When_ContentType_Is_Not_Supported()
    {
        var image = new PetImage();

        Should.Throw<DomainException>(() => image.SetImage("images/photo.jpg", "image/webp"));
    }

    [Fact]
    public void SetDisplayOrder_Should_Set_Order_When_Valid()
    {
        var image = new PetImage();

        image.SetDisplayOrder(2);

        image.DisplayOrder.ShouldBe(2);
    }

    [Fact]
    public void SetDisplayOrder_Should_Accept_Zero()
    {
        var image = new PetImage();

        Should.NotThrow(() => image.SetDisplayOrder(0));
    }

    [Fact]
    public void SetDisplayOrder_Should_Throw_When_Negative()
    {
        var image = new PetImage();

        Should.Throw<DomainException>(() => image.SetDisplayOrder(-1));
    }
}
