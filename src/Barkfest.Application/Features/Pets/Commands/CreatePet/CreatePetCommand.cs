using Barkfest.Application.Common;
using Barkfest.Application.Common.Interfaces;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Enums;
using Barkfest.Domain.Errors;
using Barkfest.Domain.Interfaces;
using CSharpFunctionalExtensions;
using MediatR;

namespace Barkfest.Application.Features.Pets.Commands.CreatePet;

public record CreatePetCommand(
    string Name,
    string? Description,
    DateOnly? DateOfBirth,
    int PetTypeValue,
    int BreedValue) : IRequest<Result<Guid, Error>>;

public class CreatePetCommandHandler(
    IOwnerRepository ownerRepository,
    IPetRepository petRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService)
    : IRequestHandler<CreatePetCommand, Result<Guid, Error>>
{
    public async Task<Result<Guid, Error>> Handle(CreatePetCommand request, CancellationToken cancellationToken)
    {
        var ownerId = currentUserService.OwnerId;
        if (ownerId is null)
            return new NotFoundError(nameof(Owner), "unknown");

        var owner = await ownerRepository.GetByIdAsync(ownerId.Value, cancellationToken);

        if (owner is null)
            return new NotFoundError(nameof(Owner), ownerId.Value);

        var petType = PetType.FromValue(request.PetTypeValue);

        // Lift domain construction (Pet.Create may throw DomainException, e.g. an invalid
        // breed for the type) into the railway via the single boundary adapter.
        var petResult = DomainResult.Try(() =>
            Pet.Create(ownerId.Value, request.Name, petType, request.BreedValue, request.Description, request.DateOfBirth));

        if (petResult.IsFailure)
            return petResult.Error;

        await petRepository.AddAsync(petResult.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return petResult.Value.Id;
    }
}
