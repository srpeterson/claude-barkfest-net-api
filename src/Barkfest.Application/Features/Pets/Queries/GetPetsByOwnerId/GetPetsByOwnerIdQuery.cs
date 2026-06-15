using Barkfest.Application.Common.Interfaces;
using Barkfest.Application.Features.Pets.DTOs;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Errors;
using Barkfest.Domain.Interfaces;
using CSharpFunctionalExtensions;
using MediatR;

namespace Barkfest.Application.Features.Pets.Queries.GetPetsByOwnerId;

public record GetPetsByOwnerIdQuery(Guid OwnerId) : IRequest<Result<IEnumerable<PetDto>, Error>>;

public class GetPetsByOwnerIdQueryHandler(
    IPetRepository petRepository,
    IOwnerRepository ownerRepository,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetPetsByOwnerIdQuery, Result<IEnumerable<PetDto>, Error>>
{
    public async Task<Result<IEnumerable<PetDto>, Error>> Handle(GetPetsByOwnerIdQuery request, CancellationToken cancellationToken)
    {
        var owner = await ownerRepository.GetByIdAsync(request.OwnerId, cancellationToken);

        if (owner is null || (!owner.IsActive && !currentUserService.IsAdmin))
            return new NotFoundError(nameof(Owner), request.OwnerId);

        if (!owner.IsVisible && !currentUserService.IsAdmin && currentUserService.OwnerId != owner.Id)
            return new NotFoundError(nameof(Owner), request.OwnerId);

        var pets = await petRepository.GetByOwnerIdAsync(request.OwnerId, cancellationToken);
        return Result.Success<IEnumerable<PetDto>, Error>(pets.ToDtoList());
    }
}
