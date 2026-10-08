using Barkfest.Application.Common.Interfaces;
using Barkfest.Application.Common.Models;
using Barkfest.Application.Features.Browse.DTOs;
using Barkfest.Domain.Enums;
using Barkfest.Domain.Errors;
using CSharpFunctionalExtensions;
using MediatR;

namespace Barkfest.Application.Features.Browse.Queries;

public record GetBrowseImagesQuery(int? PetTypeValue, int? BreedValue, int Page, int PageSize)
    : IRequest<Result<PagedResult<BrowseImageDto>, Error>>;

public class GetBrowseImagesQueryHandler(IBrowseRepository browseRepository)
    : IRequestHandler<GetBrowseImagesQuery, Result<PagedResult<BrowseImageDto>, Error>>
{
    public async Task<Result<PagedResult<BrowseImageDto>, Error>> Handle(
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
