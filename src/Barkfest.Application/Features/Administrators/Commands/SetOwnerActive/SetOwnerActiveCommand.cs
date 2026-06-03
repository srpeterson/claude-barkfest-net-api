using Barkfest.Application.Common.Exceptions;
using Barkfest.Application.Common.Interfaces;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Exceptions;
using Barkfest.Domain.Interfaces;
using MediatR;

namespace Barkfest.Application.Features.Administrators.Commands.SetOwnerActive;

public record SetOwnerActiveCommand(Guid OwnerId, bool IsActive) : IRequest;

public class SetOwnerActiveCommandHandler(
    IOwnerRepository ownerRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService)
    : IRequestHandler<SetOwnerActiveCommand>
{
    public async Task Handle(SetOwnerActiveCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAdmin)
            throw new ForbiddenException();

        var owner = await ownerRepository.GetByIdAsync(request.OwnerId, cancellationToken);

        if (owner is null)
            throw new NotFoundException(nameof(Owner), request.OwnerId);

        owner.SetIsActive(request.IsActive);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
