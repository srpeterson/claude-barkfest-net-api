using Barkfest.Application.Features.Owners.DTOs;
using Barkfest.Application.Features.Pets.DTOs;
using Barkfest.Domain.Entities;

namespace Barkfest.Application.Features.Owners;

public static class OwnerMappings
{
    public static OwnerDto ToDto(this Owner owner) => new(
        owner.Id,
        owner.Username,
        owner.FirstName,
        owner.LastName,
        owner.Email,
        owner.PhoneNumber,
        owner.IsVisible,
        owner.ProfileImage is null ? null : new ProfileImageDto(owner.ProfileImage.BlobName, owner.ProfileImage.ContentType),
        owner.CreatedAt);

    public static IEnumerable<OwnerDto> ToDtoList(this IEnumerable<Owner> owners) =>
        owners.Select(o => o.ToDto());
}
