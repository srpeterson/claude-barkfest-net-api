using Barkfest.Application.Common.Exceptions;
using Barkfest.Application.Common.Interfaces;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Exceptions;
using Barkfest.Domain.Interfaces;
using MediatR;

namespace Barkfest.Application.Features.Owners.Commands.UpdateOwner;

public record UpdateOwnerCommand(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    string? DisplayName = null) : IRequest;

public class UpdateOwnerCommandHandler(IOwnerRepository ownerRepository, IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    : IRequestHandler<UpdateOwnerCommand>
{
    public async Task Handle(UpdateOwnerCommand request, CancellationToken cancellationToken)
    {
        var owner = await ownerRepository.GetByIdAsync(request.Id, cancellationToken);

        if (owner is null)
            throw new NotFoundException(nameof(Owner), request.Id);

        if (owner.Id != currentUserService.OwnerId)
            throw new ForbiddenException();

        owner.SetFirstName(request.FirstName);
        owner.SetLastName(request.LastName);
        owner.SetEmail(request.Email);
        owner.SetPhoneNumber(request.PhoneNumber);
        owner.SetDisplayName(request.DisplayName);

        await ownerRepository.UpdateAsync(owner, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
