using Barkfest.Application.Common;
using Barkfest.Application.Common.Interfaces;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Enums;
using Barkfest.Domain.Errors;
using Barkfest.Domain.Interfaces;
using CSharpFunctionalExtensions;
using MediatR;

namespace Barkfest.Application.Features.Pets.Commands.UpdatePet;

public record UpdatePetCommand(
    Guid PetId,
    string Name,
    string? Description,
    DateOnly? DateOfBirth,
    int PetTypeValue,
    int BreedValue) : IRequest<Result<Unit, Error>>;

public class UpdatePetCommandHandler(IPetRepository petRepository, IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    : IRequestHandler<UpdatePetCommand, Result<Unit, Error>>
{
    public async Task<Result<Unit, Error>> Handle(UpdatePetCommand request, CancellationToken cancellationToken)
    {
        var pet = await petRepository.GetByIdAsync(request.PetId, cancellationToken);

        if (pet is null)
            return new NotFoundError(nameof(Pet), request.PetId);

        if (pet.OwnerId != currentUserService.OwnerId)
            return new ForbiddenError();

        var petType = PetType.FromValue(request.PetTypeValue);

        // Lift the domain mutations (setters may throw DomainException) into the railway.
        var mutation = DomainResult.Try(() =>
        {
            pet.SetName(request.Name);
            pet.SetDescription(request.Description);
            pet.SetDateOfBirth(request.DateOfBirth);
            pet.SetPetType(petType);
            pet.SetBreed(request.BreedValue);
        });

        if (mutation.IsFailure)
            return mutation.Error;

        await petRepository.UpdateAsync(pet, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
