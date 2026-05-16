using Barkfest.Application.Common.Exceptions;
using Barkfest.Application.Common.Interfaces;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Interfaces;
using MediatR;

namespace Barkfest.Application.Features.Pets.Commands.RemovePetProfileImage;

public class RemovePetProfileImageCommandHandler(
    IPetRepository petRepository,
    IBlobStorageService blobStorageService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<RemovePetProfileImageCommand>
{
    private const string ContainerName = "pet-profile-images";

    public async Task Handle(RemovePetProfileImageCommand request, CancellationToken cancellationToken)
    {
        var pet = await petRepository.GetByIdAsync(request.PetId, cancellationToken);

        if (pet is null)
            throw new NotFoundException(nameof(Pet), request.PetId);

        if (pet.ProfileImage is not null)
            await blobStorageService.DeleteAsync(ContainerName, pet.ProfileImage.BlobName, cancellationToken);

        pet.RemoveProfileImage();

        await petRepository.UpdateAsync(pet, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
