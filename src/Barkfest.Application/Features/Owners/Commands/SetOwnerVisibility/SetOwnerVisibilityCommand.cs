using Barkfest.Application.Common.Exceptions;
using Barkfest.Application.Common.Interfaces;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Exceptions;
using Barkfest.Domain.Interfaces;
using MediatR;

namespace Barkfest.Application.Features.Owners.Commands.SetOwnerVisibility;

public record SetOwnerVisibilityCommand(Guid OwnerId, bool IsVisible) : IRequest;

public class SetOwnerVisibilityCommandHandler(
    IOwnerRepository ownerRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService)
    : IRequestHandler<SetOwnerVisibilityCommand>
{
    public async Task Handle(SetOwnerVisibilityCommand request, CancellationToken cancellationToken)
    {
        var owner = await ownerRepository.GetByIdAsync(request.OwnerId, cancellationToken);

        if (owner is null)
            throw new NotFoundException(nameof(Owner), request.OwnerId);

        if (owner.Id != currentUserService.OwnerId)
            throw new ForbiddenException();

        owner.SetIsVisible(request.IsVisible);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
