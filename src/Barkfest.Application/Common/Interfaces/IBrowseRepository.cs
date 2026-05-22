using Barkfest.Application.Common.Models;
using Barkfest.Application.Features.Browse.DTOs;
using Barkfest.Domain.Enums;

namespace Barkfest.Application.Common.Interfaces;

public interface IBrowseRepository
{
    Task<PagedResult<BrowseImageDto>> GetBrowseImagesAsync(
        PetType? petType,
        string? breed,
        int page,
        int pageSize,
        CancellationToken cancellationToken);
}
