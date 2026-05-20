using Barkfest.Application.Common.Exceptions;
using Barkfest.Application.Common.Interfaces;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Exceptions;
using Barkfest.Domain.Interfaces;
using MediatR;

namespace Barkfest.Application.Features.Owners.Commands.DeleteOwner;

public record DeleteOwnerCommand(Guid Id) : IRequest;

public class DeleteOwnerCommandHandler(IOwnerRepository ownerRepository, IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    : IRequestHandler<DeleteOwnerCommand>
{
    public async Task Handle(DeleteOwnerCommand request, CancellationToken cancellationToken)
    {
        var owner = await ownerRepository.GetByIdAsync(request.Id, cancellationToken);

        if (owner is null)
            throw new NotFoundException(nameof(Owner), request.Id);

        if (owner.Id != currentUserService.OwnerId && !currentUserService.IsAdmin)
            throw new ForbiddenException();

        await ownerRepository.DeleteAsync(request.Id, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
