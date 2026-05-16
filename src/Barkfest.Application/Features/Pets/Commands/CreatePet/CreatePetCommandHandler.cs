using Barkfest.Application.Common.Exceptions;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Enums;
using Barkfest.Domain.Interfaces;
using MediatR;

namespace Barkfest.Application.Features.Pets.Commands.CreatePet;

public class CreatePetCommandHandler(
    IOwnerRepository ownerRepository,
    IPetRepository petRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreatePetCommand, Guid>
{
    public async Task<Guid> Handle(CreatePetCommand request, CancellationToken cancellationToken)
    {
        var owner = await ownerRepository.GetByIdAsync(request.OwnerId, cancellationToken);

        if (owner is null)
            throw new NotFoundException(nameof(Owner), request.OwnerId);

        var petType = PetType.FromName(request.PetType);

        var pet = new Pet(request.OwnerId);
        pet.SetName(request.Name);
        pet.SetDescription(request.Description);
        pet.SetDateOfBirth(request.DateOfBirth);
        pet.SetPetType(petType);

        await petRepository.AddAsync(pet, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return pet.Id;
    }
}
