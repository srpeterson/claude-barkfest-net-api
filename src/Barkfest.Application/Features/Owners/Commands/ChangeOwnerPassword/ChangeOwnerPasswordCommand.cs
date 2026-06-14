using Barkfest.Application.Common.Interfaces;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Errors;
using Barkfest.Domain.Interfaces;
using CSharpFunctionalExtensions;
using MediatR;

namespace Barkfest.Application.Features.Owners.Commands.ChangeOwnerPassword;

public record ChangeOwnerPasswordCommand(Guid OwnerId, string CurrentPassword, string NewPassword) : IRequest<Result<Unit, Error>>;

public class ChangeOwnerPasswordCommandHandler(
    IOwnerRepository ownerRepository,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService) : IRequestHandler<ChangeOwnerPasswordCommand, Result<Unit, Error>>
{
    public async Task<Result<Unit, Error>> Handle(ChangeOwnerPasswordCommand request, CancellationToken cancellationToken)
    {
        if (currentUserService.OwnerId != request.OwnerId)
            return new ForbiddenError();

        var owner = await ownerRepository.GetByIdAsync(request.OwnerId, cancellationToken);

        if (owner is null)
            return new NotFoundError(nameof(Owner), request.OwnerId);

        if (!passwordHasher.Verify(request.CurrentPassword, owner.PasswordHash))
            return new ForbiddenError();

        owner.SetPasswordHash(passwordHasher.Hash(request.NewPassword));
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
