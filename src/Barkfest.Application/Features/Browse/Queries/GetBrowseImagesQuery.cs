using Barkfest.Application.Common.Interfaces;
using Barkfest.Application.Common.Models;
using Barkfest.Application.Features.Browse.DTOs;
using Barkfest.Domain.Enums;
using MediatR;

namespace Barkfest.Application.Features.Browse.Queries;

public record GetBrowseImagesQuery(string? PetType, string? Breed, int Page, int PageSize)
    : IRequest<PagedResult<BrowseImageDto>>;

public class GetBrowseImagesQueryHandler(IBrowseRepository browseRepository)
    : IRequestHandler<GetBrowseImagesQuery, PagedResult<BrowseImageDto>>
{
    public async Task<PagedResult<BrowseImageDto>> Handle(
        GetBrowseImagesQuery request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(request.PetType))
        {
            var petType = PetType.List.FirstOrDefault(p =>
                p.Name.Equals(request.PetType, StringComparison.OrdinalIgnoreCase));

            // Unrecognised petType — no results
            if (petType is null)
                return new PagedResult<BrowseImageDto>([], request.Page, request.PageSize, 0);

            return await browseRepository.GetBrowseImagesAsync(
                petType, request.Breed, request.Page, request.PageSize, cancellationToken);
        }

        return await browseRepository.GetBrowseImagesAsync(
            null, request.Breed, request.Page, request.PageSize, cancellationToken);
    }
}
