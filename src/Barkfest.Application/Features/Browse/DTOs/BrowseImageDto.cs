namespace Barkfest.Application.Features.Browse.DTOs;

public record BrowseImageDto(
    Guid ImageId,
    string BlobName,
    string ContentType,
    bool IsFeaturedImage,
    DateTime CreatedAt,
    string OwnerName,
    Guid PetId,
    string PetName,
    string? PetDescription,
    DateOnly? DateOfBirth,
    int? Age,
    string PetType,
    string? Breed);
