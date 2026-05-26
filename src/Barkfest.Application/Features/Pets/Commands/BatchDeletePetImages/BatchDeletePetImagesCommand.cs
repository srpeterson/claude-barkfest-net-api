using Barkfest.Application.Common.Exceptions;
using Barkfest.Application.Common.Interfaces;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Exceptions;
using Barkfest.Domain.Interfaces;
using MediatR;

namespace Barkfest.Application.Features.Pets.Commands.BatchDeletePetImages;

public record BatchDeletePetImagesCommand(
    Guid PetId,
    IReadOnlyList<Guid> ImageIds) : IRequest;

public class BatchDeletePetImagesCommandHandler(
    IPetRepository petRepository,
    IBlobStorageService blobStorageService,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService)
    : IRequestHandler<BatchDeletePetImagesCommand>
{
    private const string ContainerName = "pet-images";

    public async Task Handle(BatchDeletePetImagesCommand request, CancellationToken cancellationToken)
    {
        var pet = await petRepository.GetByIdAsync(request.PetId, cancellationToken);

        if (pet is null)
            throw new NotFoundException(nameof(Pet), request.PetId);

        if (pet.OwnerId != currentUserService.OwnerId)
            throw new ForbiddenException();

        // Resolve blob names before RemoveImages removes them from the collection.
        var blobNames = pet.Images
            .Where(i => request.ImageIds.Contains(i.Id))
            .Select(i => i.BlobName)
            .ToList();

        // Validates all IDs exist — throws DomainException if any are missing.
        pet.RemoveImages(request.ImageIds);

        foreach (var blobName in blobNames)
            await blobStorageService.DeleteAsync(ContainerName, blobName, cancellationToken);

        await petRepository.UpdateAsync(pet, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
