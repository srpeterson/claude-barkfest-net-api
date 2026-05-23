using Barkfest.Application.Common.Interfaces;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Exceptions;
using Barkfest.Domain.Interfaces;
using MediatR;

namespace Barkfest.Application.Features.Auth.Commands.Register;

public record RegisterCommand(
    string Username,
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    string Password) : IRequest<Guid>;

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

        var owner = Owner.Create(
            request.Username,
            request.FirstName,
            request.LastName,
            request.Email,
            passwordHasher.Hash(request.Password),
            request.PhoneNumber);

        await ownerRepository.AddAsync(owner, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return owner.Id;
    }
}
