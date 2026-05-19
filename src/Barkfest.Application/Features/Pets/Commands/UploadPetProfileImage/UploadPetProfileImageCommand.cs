using Barkfest.Application.Common.Exceptions;
using Barkfest.Application.Common.Interfaces;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Exceptions;
using Barkfest.Domain.Interfaces;
using MediatR;

namespace Barkfest.Application.Features.Pets.Commands.UploadPetProfileImage;

public record UploadPetProfileImageCommand(
    Guid PetId,
    string FileName,
    Stream Content,
    string ContentType) : IRequest;

public class UploadPetProfileImageCommandHandler(
    IPetRepository petRepository,
    IBlobStorageService blobStorageService,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    IContentModerationService contentModerationService)
    : IRequestHandler<UploadPetProfileImageCommand>
{
    private const string ContainerName = "pet-profile-images";

    public async Task Handle(UploadPetProfileImageCommand request, CancellationToken cancellationToken)
    {
        var pet = await petRepository.GetByIdAsync(request.PetId, cancellationToken);

        if (pet is null)
            throw new NotFoundException(nameof(Pet), request.PetId);

        if (pet.OwnerId != currentUserService.OwnerId)
            throw new ForbiddenException();

        if (!await contentModerationService.IsImageSafeAsync(request.Content, cancellationToken))
            throw new DomainException("Image was rejected by content moderation.");

        if (pet.ProfileImage is not null)
            await blobStorageService.DeleteAsync(ContainerName, pet.ProfileImage.BlobName, cancellationToken);

        var extension = Path.GetExtension(request.FileName);
        var blobName = $"pets/{request.PetId}/profile/{Guid.CreateVersion7()}{extension}";

        await blobStorageService.UploadAsync(ContainerName, blobName, request.Content, request.ContentType, cancellationToken);

        pet.SetProfileImage(blobName, request.ContentType);

        await petRepository.UpdateAsync(pet, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
