using Barkfest.Application.Features.Pets.DTOs;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Enums;

namespace Barkfest.Application.Features.Pets;

public static class PetMappings
{
    public static PetDto ToDto(this Pet pet) => new(
        pet.Id,
        pet.Name,
        pet.Description,
        pet.DateOfBirth,
        pet.Age,
        pet.PetType.Name,
        pet.PetType == PetType.Dog
            ? DogBreed.FromValue(pet.BreedValue).Name
            : CatBreed.FromValue(pet.BreedValue).Name,
        pet.Images.Select(i => new PetImageDto(i.Id, i.BlobName, i.ContentType, i.DisplayOrder, i.IsFeaturedImage, i.CreatedAt)).ToList().AsReadOnly(),
        pet.OwnerId,
        pet.CreatedAt);

    public static IEnumerable<PetDto> ToDtoList(this IEnumerable<Pet> pets) =>
        pets.Select(p => p.ToDto());
}
