using Barkfest.Application.Common.Interfaces;
using Barkfest.Application.Common.Models;
using Barkfest.Application.Features.Browse.DTOs;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Barkfest.Persistence.Repositories;

public class BrowseRepository(AppDbContext context) : IBrowseRepository
{
    public async Task<PagedResult<BrowseImageDto>> GetBrowseImagesAsync(
        PetType? petType, string? breed, int page, int pageSize, CancellationToken cancellationToken)
    {
        var baseQuery = context.PetImages
            .Include(pi => pi.Pet)
                .ThenInclude(p => p.Owner)
            .Include(pi => pi.Pet)
                .ThenInclude(p => p.Breed)
            .Where(pi => pi.IsFeaturedImage)
            .Where(pi => pi.Pet.Owner.Active && pi.Pet.Owner.IsVisible)
            .AsQueryable();

        if (petType is not null)
            baseQuery = baseQuery.Where(pi => pi.Pet.PetType == petType);

        if (!string.IsNullOrWhiteSpace(breed))
        {
            // Resolve breed name → SmartEnum integer value for DB-level filtering.
            // Using EF.Property against the shared "BreedValue" TPH column avoids
            // loading all rows into memory before applying the breed predicate,
            // which is required for correct server-side pagination counts.
            var dogBreed = DogBreed.List.FirstOrDefault(b =>
                b.Name.Equals(breed, StringComparison.OrdinalIgnoreCase));

            var catBreed = dogBreed is null
                ? CatBreed.List.FirstOrDefault(b =>
                    b.Name.Equals(breed, StringComparison.OrdinalIgnoreCase))
                : null;

            if (dogBreed is null && catBreed is null)
                return new PagedResult<BrowseImageDto>([], page, pageSize, 0);

            var breedValue = dogBreed?.Value ?? catBreed!.Value;
            baseQuery = baseQuery.Where(pi =>
                pi.Pet.Breed != null &&
                EF.Property<int>(pi.Pet.Breed, "BreedValue") == breedValue);
        }

        var totalCount = await baseQuery.CountAsync(cancellationToken);

        var images = await baseQuery
            .OrderByDescending(pi => pi.Pet.CreatedAt)
            .AsSplitQuery()
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<BrowseImageDto>(
            images.Select(ToDto).ToList(),
            page,
            pageSize,
            totalCount);
    }

    private static BrowseImageDto ToDto(PetImage pi) => new(
        pi.Id,
        pi.BlobName,
        pi.ContentType,
        pi.IsFeaturedImage,
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
        });
}
