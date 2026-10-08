using Barkfest.Application.Features.Browse.DTOs;
using Barkfest.Domain.Enums;
using Barkfest.Domain.Errors;
using CSharpFunctionalExtensions;
using MediatR;

namespace Barkfest.Application.Features.Browse.Queries.GetBrowseBreeds;

public record GetBrowseBreedsQuery(int PetTypeValue) : IRequest<Result<IReadOnlyList<BreedOptionDto>, Error>>;

public class GetBrowseBreedsQueryHandler
    : IRequestHandler<GetBrowseBreedsQuery, Result<IReadOnlyList<BreedOptionDto>, Error>>
{
    public Task<Result<IReadOnlyList<BreedOptionDto>, Error>> Handle(
        GetBrowseBreedsQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<BreedOptionDto> breeds = PetType.TryFromValue(request.PetTypeValue, out var petType)
            ? Breed.ListFor(petType).Select(b => new BreedOptionDto(b.Name, b.Value)).ToList()
            : [];

        return Task.FromResult(Result.Success<IReadOnlyList<BreedOptionDto>, Error>(breeds));
    }
}
