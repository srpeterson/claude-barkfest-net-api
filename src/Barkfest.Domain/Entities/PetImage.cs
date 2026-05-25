using Barkfest.Domain.Exceptions;
using Barkfest.Domain.ValueObjects;

namespace Barkfest.Domain.Entities;

public class PetImage
{
    public const int BlobNameMaxLength = 500;
    public const int ContentTypeMaxLength = 100;
    public const long MaxImageSizeBytes = 10 * 1024 * 1024; // 10 MB

    public Guid Id { get; private set; } = Guid.CreateVersion7();
    public Guid PetId { get; private set; }
    public Pet Pet { get; private set; } = null!;
    public string BlobName { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public int DisplayOrder { get; private set; }
    public bool IsFeaturedImage { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public static PetImage Create(string blobName, string contentType, int displayOrder)
    {
        var image = new PetImage();
        image.SetImage(blobName, contentType);
        image.SetDisplayOrder(displayOrder);
        return image;
    }

    public void SetImage(string blobName, string contentType)
    {
        if (string.IsNullOrWhiteSpace(blobName))
            throw new DomainException("Blob name is required.");

        if (string.IsNullOrWhiteSpace(contentType))
            throw new DomainException("Content type is required.");

        if (!SupportedImageType.IsContentTypeSupported(contentType))
            throw new DomainException($"Content type '{contentType}' is not supported.");

        BlobName = blobName.Trim();
        ContentType = contentType.Trim().ToLowerInvariant();
    }

    public void SetDisplayOrder(int order)
    {
        if (order < 0)
            throw new DomainException("Display order must be zero or greater.");

        DisplayOrder = order;
    }

    public void SetAsFeatured() => IsFeaturedImage = true;

    public void UnsetAsFeatured() => IsFeaturedImage = false;
}
