using Barkfest.Application.Features.Pets.DTOs;
using Barkfest.Domain.Interfaces;
using MediatR;

namespace Barkfest.Application.Features.Pets.Queries.GetPetsByOwnerId;

public record GetPetsByOwnerIdQuery(Guid OwnerId) : IRequest<IEnumerable<PetDto>>;

public class GetPetsByOwnerIdQueryHandler(IPetRepository petRepository)
    : IRequestHandler<GetPetsByOwnerIdQuery, IEnumerable<PetDto>>
{
    public async Task<IEnumerable<PetDto>> Handle(GetPetsByOwnerIdQuery request, CancellationToken cancellationToken)
    {
        var pets = await petRepository.GetByOwnerIdAsync(request.OwnerId, cancellationToken);
        return pets.ToDtoList();
    }
}
