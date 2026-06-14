using Barkfest.Domain.Entities;
using Barkfest.Domain.Errors;
using Barkfest.Domain.Interfaces;
using CSharpFunctionalExtensions;
using MediatR;

namespace Barkfest.Application.Features.Pets.Commands.IncrementPetLikes;

public record IncrementPetLikesCommand(Guid PetId) : IRequest<Result<int, Error>>;

public class IncrementPetLikesCommandHandler(IPetRepository petRepository)
    : IRequestHandler<IncrementPetLikesCommand, Result<int, Error>>
{
    public async Task<Result<int, Error>> Handle(IncrementPetLikesCommand request, CancellationToken cancellationToken)
    {
        // Atomic relative update in the repository — no entity load, no IUnitOfWork.
        // The likes counters intentionally bypass the change tracker to avoid lost
        // updates under concurrency (see CLAUDE.md).
        var result = await petRepository.IncrementLikesAsync(request.PetId, cancellationToken);

        if (!result.PetExists)
            return new NotFoundError(nameof(Pet), request.PetId);

        return result.Likes;
    }
}
