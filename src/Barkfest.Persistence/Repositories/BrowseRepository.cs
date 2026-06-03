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
        PetType? petType, int? breedValue, int page, int pageSize, CancellationToken cancellationToken)
    {
        var baseQuery = context.PetImages
            .Include(pi => pi.Pet)
                .ThenInclude(p => p.Owner)
            .Where(pi => pi.IsFeaturedImage)
            // Public gallery only: exclude pets from deactivated or hidden owners.
            // IsActive = false means the owner account has been suspended by an administrator.
            // IsVisible = false means the owner has chosen to hide their pets from the gallery.
            // Admin views use separate endpoints and never go through this repository,
            // so they are unaffected by these filters.
            .Where(pi => pi.Pet.Owner.IsActive && pi.Pet.Owner.IsVisible)
            .AsQueryable();

        if (petType is not null)
            baseQuery = baseQuery.Where(pi => pi.Pet.PetType == petType);

        if (breedValue is not null)
            baseQuery = baseQuery.Where(pi => pi.Pet.BreedValue == breedValue);

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
        pi.Pet.Owner.DisplayName,
        pi.Pet.Id,
        pi.Pet.Name,
        pi.Pet.Description,
        pi.Pet.DateOfBirth,
        pi.Pet.Age,
        pi.Pet.PetType.Name,
        pi.Pet.PetType == PetType.Dog
            ? DogBreed.FromValue(pi.Pet.BreedValue).Name
            : CatBreed.FromValue(pi.Pet.BreedValue).Name,
        pi.Pet.Likes,
        pi.Pet.OwnerId);
}
