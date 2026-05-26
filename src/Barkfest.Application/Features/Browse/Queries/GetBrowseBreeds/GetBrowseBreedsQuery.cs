using Barkfest.Application.Features.Browse.DTOs;
using Barkfest.Domain.Enums;
using MediatR;

namespace Barkfest.Application.Features.Browse.Queries.GetBrowseBreeds;

public record GetBrowseBreedsQuery(int PetTypeValue) : IRequest<IReadOnlyList<BreedOptionDto>>;

public class GetBrowseBreedsQueryHandler : IRequestHandler<GetBrowseBreedsQuery, IReadOnlyList<BreedOptionDto>>
{
    public Task<IReadOnlyList<BreedOptionDto>> Handle(
        GetBrowseBreedsQuery request, CancellationToken cancellationToken)
    {
        if (!PetType.TryFromValue(request.PetTypeValue, out var petType))
            return Task.FromResult<IReadOnlyList<BreedOptionDto>>([]);

        IReadOnlyList<BreedOptionDto> breeds = petType == PetType.Dog
            ? DogBreed.List.OrderBy(b => b.Value).Select(b => new BreedOptionDto(b.Name, b.Value)).ToList()
            : CatBreed.List.OrderBy(b => b.Value).Select(b => new BreedOptionDto(b.Name, b.Value)).ToList();

        return Task.FromResult(breeds);
    }
}
