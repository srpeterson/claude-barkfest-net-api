using Barkfest.Application.Common.Exceptions;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Interfaces;
using MediatR;

namespace Barkfest.Application.Features.Owners.Commands.DeleteOwner;

public class DeleteOwnerCommandHandler(IOwnerRepository ownerRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteOwnerCommand>
{
    public async Task Handle(DeleteOwnerCommand request, CancellationToken cancellationToken)
    {
        var owner = await ownerRepository.GetByIdAsync(request.Id, cancellationToken);

        if (owner is null)
            throw new NotFoundException(nameof(Owner), request.Id);

        await ownerRepository.DeleteAsync(request.Id, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
