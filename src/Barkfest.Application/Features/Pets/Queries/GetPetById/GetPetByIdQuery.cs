using Barkfest.Application.Common.Interfaces;
using Barkfest.Application.Features.Pets.DTOs;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Errors;
using Barkfest.Domain.Interfaces;
using CSharpFunctionalExtensions;
using MediatR;

namespace Barkfest.Application.Features.Pets.Queries.GetPetById;

public record GetPetByIdQuery(Guid PetId) : IRequest<Result<PetDto, Error>>;

public class GetPetByIdQueryHandler(
    IPetRepository petRepository,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetPetByIdQuery, Result<PetDto, Error>>
{
    public async Task<Result<PetDto, Error>> Handle(GetPetByIdQuery request, CancellationToken cancellationToken)
    {
        // Single query loads the pet with its Images and Owner (see GetByIdWithOwnerAsync).
        var pet = await petRepository.GetByIdWithOwnerAsync(request.PetId, cancellationToken);

        if (pet is null)
            return new NotFoundError(nameof(Pet), request.PetId);

        var owner = pet.Owner;

        // A hidden/suspended owner's pet is reported as not found to non-privileged callers,
        // matching the previous behaviour (404 rather than 403, to avoid leaking existence).
        if (owner is null || (!owner.IsActive && !currentUserService.IsAdmin))
            return new NotFoundError(nameof(Pet), request.PetId);

        if (!owner.IsVisible && !currentUserService.IsAdmin && currentUserService.OwnerId != pet.OwnerId)
            return new NotFoundError(nameof(Pet), request.PetId);

        return pet.ToDto();
    }
}
