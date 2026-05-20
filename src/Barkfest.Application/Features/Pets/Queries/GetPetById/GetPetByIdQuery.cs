using Barkfest.Application.Common.Exceptions;
using Barkfest.Application.Common.Interfaces;
using Barkfest.Application.Features.Pets.DTOs;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Interfaces;
using MediatR;

namespace Barkfest.Application.Features.Pets.Queries.GetPetById;

public record GetPetByIdQuery(Guid Id) : IRequest<PetDto>;

public class GetPetByIdQueryHandler(
    IPetRepository petRepository,
    IOwnerRepository ownerRepository,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetPetByIdQuery, PetDto>
{
    public async Task<PetDto> Handle(GetPetByIdQuery request, CancellationToken cancellationToken)
    {
        var pet = await petRepository.GetByIdAsync(request.Id, cancellationToken);

        if (pet is null)
            throw new NotFoundException(nameof(Pet), request.Id);

        var owner = await ownerRepository.GetByIdAsync(pet.OwnerId, cancellationToken);

        if (owner is null || (!owner.Active && !currentUserService.IsAdmin))
            throw new NotFoundException(nameof(Pet), request.Id);

        if (!owner.IsVisible && !currentUserService.IsAdmin && currentUserService.OwnerId != pet.OwnerId)
            throw new NotFoundException(nameof(Pet), request.Id);

        return pet.ToDto();
    }
}
