using Barkfest.Domain.Enums;
using MediatR;

namespace Barkfest.Application.Features.Browse.Queries.GetBrowseBreeds;

public record GetBrowseBreedsQuery(string PetType) : IRequest<IReadOnlyList<string>>;

public class GetBrowseBreedsQueryHandler : IRequestHandler<GetBrowseBreedsQuery, IReadOnlyList<string>>
{
    public Task<IReadOnlyList<string>> Handle(
        GetBrowseBreedsQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<string> breeds;

        if (request.PetType.Equals("Dog", StringComparison.OrdinalIgnoreCase))
            breeds = DogBreed.List.OrderBy(b => b.Value).Select(b => b.Name).ToList();
        else if (request.PetType.Equals("Cat", StringComparison.OrdinalIgnoreCase))
            breeds = CatBreed.List.OrderBy(b => b.Value).Select(b => b.Name).ToList();
        else
            breeds = [];

        return Task.FromResult(breeds);
    }
}
