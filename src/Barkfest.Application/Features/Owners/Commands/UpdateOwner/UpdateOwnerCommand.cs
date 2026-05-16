using Barkfest.Application.Common.Exceptions;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Interfaces;
using MediatR;

namespace Barkfest.Application.Features.Owners.Commands.UpdateOwner;

public record UpdateOwnerCommand(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber) : IRequest;

public class UpdateOwnerCommandHandler(IOwnerRepository ownerRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateOwnerCommand>
{
    public async Task Handle(UpdateOwnerCommand request, CancellationToken cancellationToken)
    {
        var owner = await ownerRepository.GetByIdAsync(request.Id, cancellationToken);

        if (owner is null)
            throw new NotFoundException(nameof(Owner), request.Id);

        owner.SetFirstName(request.FirstName);
        owner.SetLastName(request.LastName);
        owner.SetEmail(request.Email);
        owner.SetPhoneNumber(request.PhoneNumber);

        await ownerRepository.UpdateAsync(owner, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
