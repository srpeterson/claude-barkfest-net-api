using Barkfest.Application.Common.Interfaces;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Errors;
using Barkfest.Domain.Interfaces;
using CSharpFunctionalExtensions;
using MediatR;

namespace Barkfest.Application.Features.Owners.Commands.DeleteOwner;

public record DeleteOwnerCommand(Guid OwnerId) : IRequest<Result<Unit, Error>>;

public class DeleteOwnerCommandHandler(IOwnerRepository ownerRepository, IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    : IRequestHandler<DeleteOwnerCommand, Result<Unit, Error>>
{
    public async Task<Result<Unit, Error>> Handle(DeleteOwnerCommand request, CancellationToken cancellationToken)
    {
        var owner = await ownerRepository.GetByIdAsync(request.OwnerId, cancellationToken);

        if (owner is null)
            return new NotFoundError(nameof(Owner), request.OwnerId);

        if (owner.Id != currentUserService.OwnerId && !currentUserService.IsAdmin)
            return new ForbiddenError();

        await ownerRepository.DeleteAsync(request.OwnerId, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
