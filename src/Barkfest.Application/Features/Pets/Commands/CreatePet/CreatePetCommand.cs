using Barkfest.Application.Common.Exceptions;
using Barkfest.Application.Common.Interfaces;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Enums;
using Barkfest.Domain.Interfaces;
using MediatR;

namespace Barkfest.Application.Features.Pets.Commands.CreatePet;

public record CreatePetCommand(
    string Name,
    string? Description,
    DateOnly? DateOfBirth,
    string PetType,
    string Breed) : IRequest<Guid>;

public class CreatePetCommandHandler(
    IOwnerRepository ownerRepository,
    IPetRepository petRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService)
    : IRequestHandler<CreatePetCommand, Guid>
{
    public async Task<Guid> Handle(CreatePetCommand request, CancellationToken cancellationToken)
    {
        var ownerId = currentUserService.OwnerId
            ?? throw new NotFoundException(nameof(Owner), "unknown");

        var owner = await ownerRepository.GetByIdAsync(ownerId, cancellationToken);

        if (owner is null)
            throw new NotFoundException(nameof(Owner), ownerId);

        var petType = PetType.FromName(request.PetType);

        var breedValue = petType == PetType.Dog
            ? DogBreed.FromName(request.Breed).Value
            : CatBreed.FromName(request.Breed).Value;

        var pet = Pet.Create(ownerId, request.Name, petType, breedValue, request.Description, request.DateOfBirth);

        await petRepository.AddAsync(pet, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return pet.Id;
    }
}
