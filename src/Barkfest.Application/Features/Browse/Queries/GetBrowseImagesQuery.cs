using Barkfest.Application.Common.Interfaces;
using Barkfest.Application.Common.Models;
using Barkfest.Application.Features.Browse.DTOs;
using Barkfest.Domain.Enums;
using MediatR;

namespace Barkfest.Application.Features.Browse.Queries;

public record GetBrowseImagesQuery(int? PetTypeValue, int? BreedValue, int Page, int PageSize)
    : IRequest<PagedResult<BrowseImageDto>>;

public class GetBrowseImagesQueryHandler(IBrowseRepository browseRepository)
    : IRequestHandler<GetBrowseImagesQuery, PagedResult<BrowseImageDto>>
{
    public async Task<PagedResult<BrowseImageDto>> Handle(
        GetBrowseImagesQuery request, CancellationToken cancellationToken)
    {
        if (request.PetTypeValue is not null)
        {
            if (!PetType.TryFromValue(request.PetTypeValue.Value, out var petType))
                return new PagedResult<BrowseImageDto>([], request.Page, request.PageSize, 0);

            return await browseRepository.GetBrowseImagesAsync(
                petType, request.BreedValue, request.Page, request.PageSize, cancellationToken);
        }

        return await browseRepository.GetBrowseImagesAsync(
            null, request.BreedValue, request.Page, request.PageSize, cancellationToken);
    }
}
