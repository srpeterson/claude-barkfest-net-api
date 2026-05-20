using Barkfest.Application.Common.Exceptions;
using Barkfest.Application.Common.Interfaces;
using Barkfest.Application.Features.Pets.DTOs;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Interfaces;
using MediatR;

namespace Barkfest.Application.Features.Pets.Queries.GetPetsByOwnerId;

public record GetPetsByOwnerIdQuery(Guid OwnerId) : IRequest<IEnumerable<PetDto>>;

public class GetPetsByOwnerIdQueryHandler(
    IPetRepository petRepository,
    IOwnerRepository ownerRepository,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetPetsByOwnerIdQuery, IEnumerable<PetDto>>
{
    public async Task<IEnumerable<PetDto>> Handle(GetPetsByOwnerIdQuery request, CancellationToken cancellationToken)
    {
        var owner = await ownerRepository.GetByIdAsync(request.OwnerId, cancellationToken);

        if (owner is null || (!owner.Active && !currentUserService.IsAdmin))
            throw new NotFoundException(nameof(Owner), request.OwnerId);

        if (!owner.IsVisible && !currentUserService.IsAdmin && currentUserService.OwnerId != owner.Id)
            throw new NotFoundException(nameof(Owner), request.OwnerId);

        var pets = await petRepository.GetByOwnerIdAsync(request.OwnerId, cancellationToken);
        return pets.ToDtoList();
    }
}
