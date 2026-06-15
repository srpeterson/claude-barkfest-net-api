using Barkfest.Application.Common;
using Barkfest.Application.Common.Interfaces;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Errors;
using Barkfest.Domain.Interfaces;
using CSharpFunctionalExtensions;
using MediatR;

namespace Barkfest.Application.Features.Owners.Commands.UploadOwnerProfileImage;

public record UploadOwnerProfileImageCommand(
    Guid OwnerId,
    string FileName,
    Stream Content,
    string ContentType) : IRequest<Result<Unit, Error>>;

public class UploadOwnerProfileImageCommandHandler(
    IOwnerRepository ownerRepository,
    IBlobStorageService blobStorageService,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    IContentModerationService contentModerationService)
    : IRequestHandler<UploadOwnerProfileImageCommand, Result<Unit, Error>>
{
    private const string ContainerName = BlobContainers.OwnerProfileImages;

    public async Task<Result<Unit, Error>> Handle(UploadOwnerProfileImageCommand request, CancellationToken cancellationToken)
    {
        var owner = await ownerRepository.GetByIdAsync(request.OwnerId, cancellationToken);

        if (owner is null)
            return new NotFoundError(nameof(Owner), request.OwnerId);

        if (owner.Id != currentUserService.OwnerId)
            return new ForbiddenError();

        if (!await contentModerationService.IsImageSafeAsync(request.Content, cancellationToken))
            return new DomainRuleError("Image was rejected by content moderation.");

        var oldBlobName = owner.ProfileImage?.BlobName;

        var extension = Path.GetExtension(request.FileName);
        var blobName = $"owners/{request.OwnerId}/{Guid.CreateVersion7()}{extension}";

        await blobStorageService.UploadAsync(ContainerName, blobName, request.Content, request.ContentType, cancellationToken);

        owner.SetProfileImage(blobName, request.ContentType);

        try
        {
            await ownerRepository.UpdateAsync(owner, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            // Compensate the just-uploaded blob if the DB commit fails (orphaned blob is safe).
            await blobStorageService.DeleteAsync(ContainerName, blobName, CancellationToken.None);
            throw;
        }

        // The DB now points at the new blob; delete the previous one (orphan-safe, post-commit).
        if (oldBlobName is not null)
            await blobStorageService.DeleteAsync(ContainerName, oldBlobName, cancellationToken);

        return Unit.Value;
    }
}
