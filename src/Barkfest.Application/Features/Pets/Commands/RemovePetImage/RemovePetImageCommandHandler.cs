using Barkfest.Application.Common.Exceptions;
using Barkfest.Application.Common.Interfaces;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Interfaces;
using MediatR;

namespace Barkfest.Application.Features.Pets.Commands.RemovePetImage;

public class RemovePetImageCommandHandler(
    IPetRepository petRepository,
    IBlobStorageService blobStorageService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<RemovePetImageCommand>
{
    private const string ContainerName = "pet-images";

    public async Task Handle(RemovePetImageCommand request, CancellationToken cancellationToken)
    {
        var pet = await petRepository.GetByIdAsync(request.PetId, cancellationToken);

        if (pet is null)
            throw new NotFoundException(nameof(Pet), request.PetId);

        var image = pet.Images.FirstOrDefault(i => i.Id == request.ImageId);

        if (image is null)
            throw new NotFoundException(nameof(PetImage), request.ImageId);

        await blobStorageService.DeleteAsync(ContainerName, image.BlobName, cancellationToken);

        pet.RemoveImage(request.ImageId);

        await petRepository.UpdateAsync(pet, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
