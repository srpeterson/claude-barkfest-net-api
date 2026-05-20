using Barkfest.Application.Common.Interfaces;
using Barkfest.Application.Features.Browse.DTOs;
using Barkfest.Application.Features.Pets.DTOs;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Barkfest.Persistence.Repositories;

public class BrowseRepository(AppDbContext context) : IBrowseRepository
{
    public async Task<IEnumerable<BrowseImageDto>> GetBrowseImagesAsync(
        PetType? petType, string? breed, CancellationToken cancellationToken)
    {
        var query = context.PetImages
            .AsSplitQuery()
            .Include(pi => pi.Pet)
                .ThenInclude(p => p.Owner)
            .Include(pi => pi.Pet)
                .ThenInclude(p => p.Breed)
            .Where(pi => pi.Pet.Owner.Active && pi.Pet.Owner.IsVisible)
            .OrderByDescending(pi => pi.CreatedAt)
            .AsQueryable();

        if (petType is not null)
            query = query.Where(pi => pi.Pet.PetType == petType);

        var images = await query.ToListAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(breed))
            images = images.Where(pi => MatchesBreed(pi.Pet.Breed, breed)).ToList();

        return images.Select(ToDto);
    }

    private static BrowseImageDto ToDto(PetImage pi) => new(
        pi.Id,
        pi.BlobName,
        pi.ContentType,
        pi.CreatedAt,
        $"{pi.Pet.Owner.FirstName} {pi.Pet.Owner.LastName}",
        pi.Pet.Id,
        pi.Pet.Name,
        pi.Pet.Description,
        pi.Pet.DateOfBirth,
        pi.Pet.Age,
        pi.Pet.PetType.Name,
        pi.Pet.Breed switch
        {
            DogBreedInfo dog => dog.DogBreed.Name,
            CatBreedInfo cat => cat.CatBreed.Name,
            _ => null
        },
        pi.Pet.ProfileImage is null
            ? null
            : new ProfileImageDto(pi.Pet.ProfileImage.BlobName, pi.Pet.ProfileImage.ContentType));

    private static bool MatchesBreed(Breed? breed, string breedName) =>
        breed switch
        {
            DogBreedInfo dog => dog.DogBreed.Name.Equals(breedName, StringComparison.OrdinalIgnoreCase),
            CatBreedInfo cat => cat.CatBreed.Name.Equals(breedName, StringComparison.OrdinalIgnoreCase),
            _ => false
        };
}
