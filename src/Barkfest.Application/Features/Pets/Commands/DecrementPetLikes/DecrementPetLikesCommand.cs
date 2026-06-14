using Barkfest.Domain.Entities;
using Barkfest.Domain.Errors;
using Barkfest.Domain.Interfaces;
using CSharpFunctionalExtensions;
using MediatR;

namespace Barkfest.Application.Features.Pets.Commands.DecrementPetLikes;

public record DecrementPetLikesCommand(Guid PetId) : IRequest<Result<int, Error>>;

public class DecrementPetLikesCommandHandler(IPetRepository petRepository)
    : IRequestHandler<DecrementPetLikesCommand, Result<int, Error>>
{
    public async Task<Result<int, Error>> Handle(DecrementPetLikesCommand request, CancellationToken cancellationToken)
    {
        // Atomic relative update that floors at zero in the repository — no entity
        // load, no IUnitOfWork. The likes counters intentionally bypass the change
        // tracker to avoid lost updates under concurrency (see CLAUDE.md).
        var result = await petRepository.DecrementLikesAsync(request.PetId, cancellationToken);

        if (!result.PetExists)
            return new NotFoundError(nameof(Pet), request.PetId);

        return result.Likes;
    }
}
