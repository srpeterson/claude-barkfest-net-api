using Barkfest.Application.Common.Exceptions;
using Barkfest.Application.Common.Interfaces;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Exceptions;
using Barkfest.Domain.Interfaces;
using MediatR;

namespace Barkfest.Application.Features.Owners.Commands.UploadOwnerProfileImage;

public record UploadOwnerProfileImageCommand(
    Guid OwnerId,
    string FileName,
    Stream Content,
    string ContentType) : IRequest;

public class UploadOwnerProfileImageCommandHandler(
    IOwnerRepository ownerRepository,
    IBlobStorageService blobStorageService,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    IContentModerationService contentModerationService)
    : IRequestHandler<UploadOwnerProfileImageCommand>
{
    private const string ContainerName = "owner-profile-images";

    public async Task Handle(UploadOwnerProfileImageCommand request, CancellationToken cancellationToken)
    {
        var owner = await ownerRepository.GetByIdAsync(request.OwnerId, cancellationToken);

        if (owner is null)
            throw new NotFoundException(nameof(Owner), request.OwnerId);

        if (owner.Id != currentUserService.OwnerId)
            throw new ForbiddenException();

        if (!await contentModerationService.IsImageSafeAsync(request.Content, cancellationToken))
            throw new DomainException("Image was rejected by content moderation.");

        if (owner.ProfileImage is not null)
            await blobStorageService.DeleteAsync(ContainerName, owner.ProfileImage.BlobName, cancellationToken);

        var extension = Path.GetExtension(request.FileName);
        var blobName = $"owners/{request.OwnerId}/{Guid.CreateVersion7()}{extension}";

        await blobStorageService.UploadAsync(ContainerName, blobName, request.Content, request.ContentType, cancellationToken);

        owner.SetProfileImage(blobName, request.ContentType);

        await ownerRepository.UpdateAsync(owner, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
