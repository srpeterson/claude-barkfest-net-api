namespace Barkfest.Application.Features.Pets.DTOs;

public record PetDto(
    Guid Id,
    string Name,
    string? Description,
    DateOnly? DateOfBirth,
    int? Age,
    string PetType,
    string? Breed,
    IReadOnlyCollection<PetImageDto> Images,
    Guid OwnerId,
    DateTime CreatedAt);
