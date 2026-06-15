using Barkfest.Application.Common;
using Barkfest.Application.Common.Interfaces;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Errors;
using Barkfest.Domain.Interfaces;
using CSharpFunctionalExtensions;
using MediatR;

namespace Barkfest.Application.Features.Auth.Commands.Register;

public record RegisterCommand(
    string Username,
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    string Password,
    string? DisplayName = null) : IRequest<Result<Guid, Error>>;

public class RegisterCommandHandler(
    IOwnerRepository ownerRepository,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork) : IRequestHandler<RegisterCommand, Result<Guid, Error>>
{
    public async Task<Result<Guid, Error>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var existingByUsername = await ownerRepository.GetByUsernameAsync(request.Username, cancellationToken);
        if (existingByUsername is not null)
            return new DomainRuleError("That username is already taken.");

        var existingByEmail = await ownerRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existingByEmail is not null)
            return new DomainRuleError("An account with this email address already exists.");

        if (!string.IsNullOrWhiteSpace(request.DisplayName))
        {
            var normalized = Owner.Normalize(request.DisplayName);
            if (!await ownerRepository.IsDisplayNameAvailableAsync(normalized, cancellationToken: cancellationToken))
                return new DomainRuleError("That display name is already taken.");
        }

        var creation = DomainResult.Try(() =>
        {
            var owner = Owner.Create(
                request.Username,
                request.FirstName,
                request.LastName,
                request.Email,
                passwordHasher.Hash(request.Password),
                request.PhoneNumber);

            owner.SetDisplayName(request.DisplayName);
            return owner;
        });

        if (creation.IsFailure)
            return creation.Error;

        await ownerRepository.AddAsync(creation.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return creation.Value.Id;
    }
}
