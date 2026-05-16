using Barkfest.Application.Common.Exceptions;
using Barkfest.Application.Common.Interfaces;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Interfaces;
using MediatR;

namespace Barkfest.Application.Features.Pets.Commands.UploadPetProfileImage;

public class UploadPetProfileImageCommandHandler(
    IPetRepository petRepository,
    IBlobStorageService blobStorageService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UploadPetProfileImageCommand>
{
    private const string ContainerName = "pet-profile-images";

    public async Task Handle(UploadPetProfileImageCommand request, CancellationToken cancellationToken)
    {
        var pet = await petRepository.GetByIdAsync(request.PetId, cancellationToken);

        if (pet is null)
            throw new NotFoundException(nameof(Pet), request.PetId);

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
