using Barkfest.Application.Common.Interfaces;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Errors;
using Barkfest.Domain.Interfaces;
using CSharpFunctionalExtensions;
using MediatR;

namespace Barkfest.Application.Features.Administrators.Commands.SetOwnerActive;

public record SetOwnerActiveCommand(Guid OwnerId, bool IsActive) : IRequest<Result<Unit, Error>>;

public class SetOwnerActiveCommandHandler(
    IOwnerRepository ownerRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService)
    : IRequestHandler<SetOwnerActiveCommand, Result<Unit, Error>>
{
    public async Task<Result<Unit, Error>> Handle(SetOwnerActiveCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAdmin)
            return new ForbiddenError();

        var owner = await ownerRepository.GetByIdAsync(request.OwnerId, cancellationToken);

        if (owner is null)
            return new NotFoundError(nameof(Owner), request.OwnerId);

        owner.SetIsActive(request.IsActive);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
