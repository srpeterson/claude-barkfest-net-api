namespace Barkfest.Application.Features.Pets.DTOs;

public record PetImageDto(
    Guid PetImageId,
    string BlobName,
    string ContentType,
    int DisplayOrder,
    bool IsFeaturedImage,
    DateTime CreatedAt);
