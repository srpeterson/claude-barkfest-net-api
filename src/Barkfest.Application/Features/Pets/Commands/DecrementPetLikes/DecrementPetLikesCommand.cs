using Barkfest.Application.Common.Exceptions;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Interfaces;
using MediatR;

namespace Barkfest.Application.Features.Pets.Commands.DecrementPetLikes;

public record DecrementPetLikesCommand(Guid PetId) : IRequest<int>;

public class DecrementPetLikesCommandHandler(
    IPetRepository petRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DecrementPetLikesCommand, int>
{
    public async Task<int> Handle(DecrementPetLikesCommand request, CancellationToken cancellationToken)
    {
        var pet = await petRepository.GetByIdAsync(request.PetId, cancellationToken);

        if (pet is null)
            throw new NotFoundException(nameof(Pet), request.PetId);

        pet.DecrementLikes();

        await petRepository.UpdateAsync(pet, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return pet.Likes;
    }
}
