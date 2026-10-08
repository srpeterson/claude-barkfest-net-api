using Barkfest.Application.Features.Browse.DTOs;
using Barkfest.Domain.Enums;
using Barkfest.Domain.Errors;
using CSharpFunctionalExtensions;
using MediatR;

namespace Barkfest.Application.Features.Browse.Queries.GetBrowsePetTypes;

public record GetBrowsePetTypesQuery : IRequest<Result<IReadOnlyList<PetTypeOptionDto>, Error>>;

public class GetBrowsePetTypesQueryHandler
    : IRequestHandler<GetBrowsePetTypesQuery, Result<IReadOnlyList<PetTypeOptionDto>, Error>>
{
    public Task<Result<IReadOnlyList<PetTypeOptionDto>, Error>> Handle(
        GetBrowsePetTypesQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<PetTypeOptionDto> petTypes = PetType.List
            .OrderBy(pt => pt.Value)
            .Select(pt => new PetTypeOptionDto(pt.Name, pt.Value))
            .ToList();

        return Task.FromResult(Result.Success<IReadOnlyList<PetTypeOptionDto>, Error>(petTypes));
    }
}
