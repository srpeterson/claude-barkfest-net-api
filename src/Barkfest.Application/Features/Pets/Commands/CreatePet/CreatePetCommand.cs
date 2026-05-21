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

        Breed breed;
        if (petType == PetType.Dog)
        {
            var dogBreedInfo = new DogBreedInfo();
            dogBreedInfo.SetDogBreed(DogBreed.FromName(request.Breed));
            breed = dogBreedInfo;
        }
        else
        {
            var catBreedInfo = new CatBreedInfo();
            catBreedInfo.SetCatBreed(CatBreed.FromName(request.Breed));
            breed = catBreedInfo;
        }

        var pet = Pet.Create(ownerId, request.Name, petType, breed, request.Description, request.DateOfBirth);

        await petRepository.AddAsync(pet, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return pet.Id;
    }
}
