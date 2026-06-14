using Barkfest.Application.Common.Interfaces;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Errors;
using Barkfest.Domain.Interfaces;
using CSharpFunctionalExtensions;
using MediatR;

namespace Barkfest.Application.Features.Owners.Commands.SetOwnerVisibility;

public record SetOwnerVisibilityCommand(Guid OwnerId, bool IsVisible) : IRequest<Result<Unit, Error>>;

public class SetOwnerVisibilityCommandHandler(
    IOwnerRepository ownerRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService)
    : IRequestHandler<SetOwnerVisibilityCommand, Result<Unit, Error>>
{
    public async Task<Result<Unit, Error>> Handle(SetOwnerVisibilityCommand request, CancellationToken cancellationToken)
    {
        var owner = await ownerRepository.GetByIdAsync(request.OwnerId, cancellationToken);

        if (owner is null)
            return new NotFoundError(nameof(Owner), request.OwnerId);

        if (owner.Id != currentUserService.OwnerId)
            return new ForbiddenError();

        owner.SetIsVisible(request.IsVisible);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
