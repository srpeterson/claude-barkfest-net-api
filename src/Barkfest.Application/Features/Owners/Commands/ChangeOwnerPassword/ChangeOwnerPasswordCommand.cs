using Barkfest.Application.Common.Exceptions;
using Barkfest.Application.Common.Interfaces;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Exceptions;
using Barkfest.Domain.Interfaces;
using MediatR;

namespace Barkfest.Application.Features.Owners.Commands.ChangeOwnerPassword;

public record ChangeOwnerPasswordCommand(Guid OwnerId, string CurrentPassword, string NewPassword) : IRequest;

public class ChangeOwnerPasswordCommandHandler(
    IOwnerRepository ownerRepository,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService) : IRequestHandler<ChangeOwnerPasswordCommand>
{
    public async Task Handle(ChangeOwnerPasswordCommand request, CancellationToken cancellationToken)
    {
        if (currentUserService.OwnerId != request.OwnerId)
            throw new ForbiddenException();

        var owner = await ownerRepository.GetByIdAsync(request.OwnerId, cancellationToken);

        if (owner is null)
            throw new NotFoundException(nameof(Owner), request.OwnerId);

        if (!passwordHasher.Verify(request.CurrentPassword, owner.PasswordHash))
            throw new ForbiddenException();

        owner.SetPasswordHash(passwordHasher.Hash(request.NewPassword));
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
