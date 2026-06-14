using Barkfest.Application.Common.Exceptions;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Interfaces;
using MediatR;

namespace Barkfest.Application.Features.Pets.Commands.IncrementPetLikes;

public record IncrementPetLikesCommand(Guid PetId) : IRequest<int>;

public class IncrementPetLikesCommandHandler(IPetRepository petRepository)
    : IRequestHandler<IncrementPetLikesCommand, int>
{
    public async Task<int> Handle(IncrementPetLikesCommand request, CancellationToken cancellationToken)
    {
        // Atomic relative update in the repository — no entity load, no IUnitOfWork.
        // The likes counters intentionally bypass the change tracker to avoid lost
        // updates under concurrency (see CLAUDE.md).
        var result = await petRepository.IncrementLikesAsync(request.PetId, cancellationToken);

        if (!result.PetExists)
            throw new NotFoundException(nameof(Pet), request.PetId);

        return result.Likes;
    }
}
