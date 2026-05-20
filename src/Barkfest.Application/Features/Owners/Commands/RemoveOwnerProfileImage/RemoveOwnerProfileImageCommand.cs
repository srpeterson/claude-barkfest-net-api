using Barkfest.Application.Common.Exceptions;
using Barkfest.Application.Common.Interfaces;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Exceptions;
using Barkfest.Domain.Interfaces;
using MediatR;

namespace Barkfest.Application.Features.Owners.Commands.RemoveOwnerProfileImage;

public record RemoveOwnerProfileImageCommand(Guid OwnerId) : IRequest;

public class RemoveOwnerProfileImageCommandHandler(
    IOwnerRepository ownerRepository,
    IBlobStorageService blobStorageService,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService)
    : IRequestHandler<RemoveOwnerProfileImageCommand>
{
    private const string ContainerName = "owner-profile-images";

    public async Task Handle(RemoveOwnerProfileImageCommand request, CancellationToken cancellationToken)
    {
        var owner = await ownerRepository.GetByIdAsync(request.OwnerId, cancellationToken);

        if (owner is null)
            throw new NotFoundException(nameof(Owner), request.OwnerId);

        if (owner.Id != currentUserService.OwnerId)
            throw new ForbiddenException();

        if (owner.ProfileImage is not null)
            await blobStorageService.DeleteAsync(ContainerName, owner.ProfileImage.BlobName, cancellationToken);

        owner.RemoveProfileImage();

        await ownerRepository.UpdateAsync(owner, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
