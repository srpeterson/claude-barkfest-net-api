using Barkfest.Application.Common.Interfaces;
using Barkfest.Application.Features.Browse.DTOs;
using Barkfest.Domain.Enums;
using MediatR;

namespace Barkfest.Application.Features.Browse.Queries;

public record GetBrowseImagesQuery(string? PetType, string? Breed) : IRequest<IEnumerable<BrowseImageDto>>;

public class GetBrowseImagesQueryHandler(IBrowseRepository browseRepository)
    : IRequestHandler<GetBrowseImagesQuery, IEnumerable<BrowseImageDto>>
{
    public async Task<IEnumerable<BrowseImageDto>> Handle(
        GetBrowseImagesQuery request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(request.PetType))
        {
            var petType = PetType.List.FirstOrDefault(p =>
                p.Name.Equals(request.PetType, StringComparison.OrdinalIgnoreCase));

            // Unrecognised petType — no results
            if (petType is null)
                return [];

            return await browseRepository.GetBrowseImagesAsync(petType, request.Breed, cancellationToken);
        }

        return await browseRepository.GetBrowseImagesAsync(null, request.Breed, cancellationToken);
    }
}
