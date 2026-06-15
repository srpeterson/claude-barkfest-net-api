using Barkfest.Application.Common;
using Barkfest.Application.Common.Interfaces;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Errors;
using Barkfest.Domain.Interfaces;
using CSharpFunctionalExtensions;
using MediatR;

namespace Barkfest.Application.Features.Owners.Commands.UpdateOwner;

public record UpdateOwnerCommand(
    Guid OwnerId,
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    string? DisplayName = null) : IRequest<Result<Unit, Error>>;

public class UpdateOwnerCommandHandler(IOwnerRepository ownerRepository, IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    : IRequestHandler<UpdateOwnerCommand, Result<Unit, Error>>
{
    public async Task<Result<Unit, Error>> Handle(UpdateOwnerCommand request, CancellationToken cancellationToken)
    {
        var owner = await ownerRepository.GetByIdAsync(request.OwnerId, cancellationToken);

        if (owner is null)
            return new NotFoundError(nameof(Owner), request.OwnerId);

        if (owner.Id != currentUserService.OwnerId)
            return new ForbiddenError();

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var existingByEmail = await ownerRepository.GetByEmailAsync(normalizedEmail, cancellationToken);
        if (existingByEmail is not null && existingByEmail.Id != request.OwnerId)
            return new DomainRuleError("An account with this email address already exists.");

        if (!string.IsNullOrWhiteSpace(request.DisplayName))
        {
            var normalizedDisplayName = Owner.Normalize(request.DisplayName);
            if (!await ownerRepository.IsDisplayNameAvailableAsync(normalizedDisplayName, request.OwnerId, cancellationToken))
                return new DomainRuleError("That display name is already taken.");
        }

        var mutation = DomainResult.Try(() =>
        {
            owner.SetFirstName(request.FirstName);
            owner.SetLastName(request.LastName);
            owner.SetEmail(request.Email);
            owner.SetPhoneNumber(request.PhoneNumber);
            owner.SetDisplayName(request.DisplayName);
        });

        if (mutation.IsFailure)
            return mutation.Error;

        await ownerRepository.UpdateAsync(owner, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
