using Barkfest.Domain.Entities;

namespace Barkfest.Tests.Common.Builders;

public class PetImageBuilder
{
    private string _blobName = "pets/test/gallery/photo.jpg";
    private string _contentType = "image/jpeg";
    private int _displayOrder = 0;
    private bool _isFeaturedImage = false;

    public PetImageBuilder WithBlobName(string blobName)
    {
        _blobName = blobName;
        return this;
    }

    public PetImageBuilder WithContentType(string contentType)
    {
        _contentType = contentType;
        return this;
    }

    public PetImageBuilder WithDisplayOrder(int order)
    {
        _displayOrder = order;
        return this;
    }

    public PetImageBuilder AsFeatured()
    {
        _isFeaturedImage = true;
        return this;
    }

    public PetImage Build()
    {
        var image = PetImage.Create(_blobName, _contentType, _displayOrder);
        if (_isFeaturedImage)
            image.SetAsFeatured();
        return image;
    }
}
