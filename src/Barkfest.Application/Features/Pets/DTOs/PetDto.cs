namespace Barkfest.Application.Features.Pets.DTOs;

public record PetDto(
    Guid PetId,
    string Name,
    string? Description,
    DateOnly? DateOfBirth,
    int? Age,
    string PetType,
    string? Breed,
    IReadOnlyCollection<PetImageDto> Images,
    Guid OwnerId,
    DateTime CreatedAt,
    int Likes);
