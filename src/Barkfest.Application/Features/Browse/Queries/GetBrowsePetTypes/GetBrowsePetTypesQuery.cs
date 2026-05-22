using Barkfest.Domain.Enums;
using MediatR;

namespace Barkfest.Application.Features.Browse.Queries.GetBrowsePetTypes;

public record GetBrowsePetTypesQuery : IRequest<IReadOnlyList<string>>;

public class GetBrowsePetTypesQueryHandler : IRequestHandler<GetBrowsePetTypesQuery, IReadOnlyList<string>>
{
    public Task<IReadOnlyList<string>> Handle(
        GetBrowsePetTypesQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<string> petTypes = PetType.List
            .OrderBy(pt => pt.Value)
            .Select(pt => pt.Name)
            .ToList();

        return Task.FromResult(petTypes);
    }
}
