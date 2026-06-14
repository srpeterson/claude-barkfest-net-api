using Barkfest.Application.Common.Interfaces;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Errors;
using Barkfest.Domain.Interfaces;
using CSharpFunctionalExtensions;
using MediatR;

namespace Barkfest.Application.Features.Owners.Commands.RemoveOwnerProfileImage;

public record RemoveOwnerProfileImageCommand(Guid OwnerId) : IRequest<Result<Unit, Error>>;

public class RemoveOwnerProfileImageCommandHandler(
    IOwnerRepository ownerRepository,
    IBlobStorageService blobStorageService,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService)
    : IRequestHandler<RemoveOwnerProfileImageCommand, Result<Unit, Error>>
{
    private const string ContainerName = "owner-profile-images";

    public async Task<Result<Unit, Error>> Handle(RemoveOwnerProfileImageCommand request, CancellationToken cancellationToken)
    {
        var owner = await ownerRepository.GetByIdAsync(request.OwnerId, cancellationToken);

        if (owner is null)
            return new NotFoundError(nameof(Owner), request.OwnerId);

        if (owner.Id != currentUserService.OwnerId)
            return new ForbiddenError();

        var blobName = owner.ProfileImage?.BlobName;
        owner.RemoveProfileImage();

        // Commit the DB change first, then delete the blob (see #6 — fail toward an
        // orphaned blob, never a row referencing a deleted blob).
        await ownerRepository.UpdateAsync(owner, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (blobName is not null)
            await blobStorageService.DeleteAsync(ContainerName, blobName, cancellationToken);

        return Unit.Value;
    }
}
