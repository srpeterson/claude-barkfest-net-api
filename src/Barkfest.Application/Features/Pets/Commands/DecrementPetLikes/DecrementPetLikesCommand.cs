using Barkfest.Application.Common.Exceptions;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Interfaces;
using MediatR;

namespace Barkfest.Application.Features.Pets.Commands.DecrementPetLikes;

public record DecrementPetLikesCommand(Guid PetId) : IRequest<int>;

public class DecrementPetLikesCommandHandler(IPetRepository petRepository)
    : IRequestHandler<DecrementPetLikesCommand, int>
{
    public async Task<int> Handle(DecrementPetLikesCommand request, CancellationToken cancellationToken)
    {
        // Atomic relative update that floors at zero in the repository — no entity
        // load, no IUnitOfWork. The likes counters intentionally bypass the change
        // tracker to avoid lost updates under concurrency (see CLAUDE.md).
        var result = await petRepository.DecrementLikesAsync(request.PetId, cancellationToken);

        if (!result.PetExists)
            throw new NotFoundException(nameof(Pet), request.PetId);

        return result.Likes;
    }
}
