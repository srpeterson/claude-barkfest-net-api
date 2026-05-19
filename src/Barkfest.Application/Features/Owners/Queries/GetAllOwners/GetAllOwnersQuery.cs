using Barkfest.Application.Common.Interfaces;
using Barkfest.Application.Features.Owners.DTOs;
using Barkfest.Domain.Exceptions;
using Barkfest.Domain.Interfaces;
using MediatR;

namespace Barkfest.Application.Features.Owners.Queries.GetAllOwners;

public record GetAllOwnersQuery : IRequest<IEnumerable<OwnerDto>>;

public class GetAllOwnersQueryHandler(
    IOwnerRepository ownerRepository,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetAllOwnersQuery, IEnumerable<OwnerDto>>
{
    public async Task<IEnumerable<OwnerDto>> Handle(GetAllOwnersQuery request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAdmin)
            throw new ForbiddenException();

        var owners = await ownerRepository.GetAllAsync(cancellationToken);
        return owners.ToDtoList();
    }
}
