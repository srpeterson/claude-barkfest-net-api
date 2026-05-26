using Barkfest.Application.Features.Browse.DTOs;
using Barkfest.Domain.Enums;
using MediatR;

namespace Barkfest.Application.Features.Browse.Queries.GetBrowsePetTypes;

public record GetBrowsePetTypesQuery : IRequest<IReadOnlyList<PetTypeOptionDto>>;

public class GetBrowsePetTypesQueryHandler : IRequestHandler<GetBrowsePetTypesQuery, IReadOnlyList<PetTypeOptionDto>>
{
    public Task<IReadOnlyList<PetTypeOptionDto>> Handle(
        GetBrowsePetTypesQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<PetTypeOptionDto> petTypes = PetType.List
            .OrderBy(pt => pt.Value)
            .Select(pt => new PetTypeOptionDto(pt.Name, pt.Value))
            .ToList();

        return Task.FromResult(petTypes);
    }
}
