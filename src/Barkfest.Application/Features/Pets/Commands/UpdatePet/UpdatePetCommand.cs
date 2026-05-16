using Barkfest.Application.Common.Exceptions;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Enums;
using Barkfest.Domain.Interfaces;
using MediatR;

namespace Barkfest.Application.Features.Pets.Commands.UpdatePet;

public record UpdatePetCommand(
    Guid Id,
    string Name,
    string? Description,
    DateOnly? DateOfBirth,
    string PetType) : IRequest;

public class UpdatePetCommandHandler(IPetRepository petRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<UpdatePetCommand>
{
    public async Task Handle(UpdatePetCommand request, CancellationToken cancellationToken)
    {
        var pet = await petRepository.GetByIdAsync(request.Id, cancellationToken);

        if (pet is null)
            throw new NotFoundException(nameof(Pet), request.Id);

        var petType = PetType.FromName(request.PetType);

        pet.SetName(request.Name);
        pet.SetDescription(request.Description);
        pet.SetDateOfBirth(request.DateOfBirth);
        pet.SetPetType(petType);

        await petRepository.UpdateAsync(pet, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
