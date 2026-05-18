using Barkfest.Application.Features.Pets.DTOs;

namespace Barkfest.Application.Features.Browse.DTOs;

public record BrowseImageDto(
    Guid ImageId,
    string BlobName,
    string ContentType,
    DateTime CreatedAt,
    string OwnerName,
    Guid PetId,
    string PetName,
    string? PetDescription,
    DateOnly? DateOfBirth,
    int? Age,
    string PetType,
    string? Breed,
    ProfileImageDto? PetProfileImage);
