using Barkfest.Domain.Entities;
using Barkfest.Domain.Interfaces;
using MediatR;

namespace Barkfest.Application.Features.Owners.Commands.CreateOwner;

public record CreateOwnerCommand(
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber) : IRequest<Guid>;

public class CreateOwnerCommandHandler(IOwnerRepository ownerRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<CreateOwnerCommand, Guid>
{
    public async Task<Guid> Handle(CreateOwnerCommand request, CancellationToken cancellationToken)
    {
        var owner = new Owner();
        owner.SetFirstName(request.FirstName);
        owner.SetLastName(request.LastName);
        owner.SetEmail(request.Email);
        owner.SetPhoneNumber(request.PhoneNumber);

        await ownerRepository.AddAsync(owner, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return owner.Id;
    }
}
