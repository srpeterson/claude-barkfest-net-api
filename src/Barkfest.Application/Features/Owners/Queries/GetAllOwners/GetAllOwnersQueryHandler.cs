using Barkfest.Application.Features.Owners.DTOs;
using Barkfest.Domain.Interfaces;
using MediatR;

namespace Barkfest.Application.Features.Owners.Queries.GetAllOwners;

public class GetAllOwnersQueryHandler(IOwnerRepository ownerRepository)
    : IRequestHandler<GetAllOwnersQuery, IEnumerable<OwnerDto>>
{
    public async Task<IEnumerable<OwnerDto>> Handle(GetAllOwnersQuery request, CancellationToken cancellationToken)
    {
        var owners = await ownerRepository.GetAllAsync(cancellationToken);
        return owners.ToDtoList();
    }
}
