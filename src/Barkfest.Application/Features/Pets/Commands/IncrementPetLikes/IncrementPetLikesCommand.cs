using Barkfest.Application.Common.Exceptions;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Interfaces;
using MediatR;

namespace Barkfest.Application.Features.Pets.Commands.IncrementPetLikes;

public record IncrementPetLikesCommand(Guid PetId) : IRequest<int>;

public class IncrementPetLikesCommandHandler(
    IPetRepository petRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<IncrementPetLikesCommand, int>
{
    public async Task<int> Handle(IncrementPetLikesCommand request, CancellationToken cancellationToken)
    {
        var pet = await petRepository.GetByIdAsync(request.PetId, cancellationToken);

        if (pet is null)
            throw new NotFoundException(nameof(Pet), request.PetId);

        pet.IncrementLikes();

        await petRepository.UpdateAsync(pet, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return pet.Likes;
    }
}
