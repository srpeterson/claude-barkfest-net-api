using Barkfest.Application.Features.Browse.DTOs;
using Barkfest.Domain.Enums;

namespace Barkfest.Application.Common.Interfaces;

public interface IBrowseRepository
{
    Task<IEnumerable<BrowseImageDto>> GetBrowseImagesAsync(
        PetType? petType,
        string? breed,
        CancellationToken cancellationToken);
}
