using Barkfest.Domain.Exceptions;

namespace Barkfest.Domain.ValueObjects;

public sealed record ProfileImage
{
    public string BlobName { get; }
    public string ContentType { get; }

    private ProfileImage(string blobName, string contentType)
    {
        BlobName = blobName;
        ContentType = contentType;
    }

    public static ProfileImage Create(string blobName, string contentType)
    {
        if (string.IsNullOrWhiteSpace(blobName))
            throw new DomainException("Blob name is required.");

        if (string.IsNullOrWhiteSpace(contentType))
            throw new DomainException("Content type is required.");

        var normalizedContentType = contentType.Trim().ToLowerInvariant();

        if (!SupportedImageType.IsContentTypeSupported(normalizedContentType))
            throw new DomainException($"Content type '{contentType}' is not supported.");

        return new ProfileImage(blobName.Trim(), normalizedContentType);
    }
}
