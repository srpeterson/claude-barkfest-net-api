using Barkfest.Application.Common.Interfaces;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Exceptions;
using Barkfest.Domain.Interfaces;
using MediatR;

namespace Barkfest.Application.Features.Auth.Commands.Register;

public class RegisterCommandHandler(
    IOwnerRepository ownerRepository,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork) : IRequestHandler<RegisterCommand, Guid>
{
    public async Task<Guid> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var existingByUsername = await ownerRepository.GetByUsernameAsync(request.Username, cancellationToken);
        if (existingByUsername is not null)
            throw new DomainException("Username is already in use.");

        var existingByEmail = await ownerRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existingByEmail is not null)
            throw new DomainException("Email is already in use.");

        var owner = new Owner();
        owner.SetUsername(request.Username);
        owner.SetFirstName(request.FirstName);
        owner.SetLastName(request.LastName);
        owner.SetEmail(request.Email);
        owner.SetPhoneNumber(request.PhoneNumber);
        owner.SetPasswordHash(passwordHasher.Hash(request.Password));

        await ownerRepository.AddAsync(owner, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return owner.Id;
    }
}
