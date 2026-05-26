using Barkfest.Application.Common.Exceptions;
using Barkfest.Application.Common.Interfaces;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Enums;
using Barkfest.Domain.Exceptions;
using Barkfest.Domain.Interfaces;
using MediatR;

namespace Barkfest.Application.Features.Pets.Commands.UpdatePet;

public record UpdatePetCommand(
    Guid Id,
    string Name,
    string? Description,
    DateOnly? DateOfBirth,
    int PetTypeValue,
    int BreedValue) : IRequest;

public class UpdatePetCommandHandler(IPetRepository petRepository, IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    : IRequestHandler<UpdatePetCommand>
{
    public async Task Handle(UpdatePetCommand request, CancellationToken cancellationToken)
    {
        var pet = await petRepository.GetByIdAsync(request.Id, cancellationToken);

        if (pet is null)
            throw new NotFoundException(nameof(Pet), request.Id);

        if (pet.OwnerId != currentUserService.OwnerId)
            throw new ForbiddenException();

        var petType = PetType.FromValue(request.PetTypeValue);

        pet.SetName(request.Name);
        pet.SetDescription(request.Description);
        pet.SetDateOfBirth(request.DateOfBirth);
        pet.SetPetType(petType);
        pet.SetBreed(request.BreedValue);

        await petRepository.UpdateAsync(pet, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
