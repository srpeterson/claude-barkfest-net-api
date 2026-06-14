using Barkfest.Application.Common;
using Barkfest.Application.Common.Interfaces;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Errors;
using Barkfest.Domain.Interfaces;
using CSharpFunctionalExtensions;
using MediatR;

namespace Barkfest.Application.Features.Pets.Commands.BatchDeletePetImages;

public record BatchDeletePetImagesCommand(
    Guid PetId,
    IReadOnlyList<Guid> ImageIds) : IRequest<Result<Unit, Error>>;

public class BatchDeletePetImagesCommandHandler(
    IPetRepository petRepository,
    IBlobStorageService blobStorageService,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService)
    : IRequestHandler<BatchDeletePetImagesCommand, Result<Unit, Error>>
{
    private const string ContainerName = "pet-images";

    public async Task<Result<Unit, Error>> Handle(BatchDeletePetImagesCommand request, CancellationToken cancellationToken)
    {
        var pet = await petRepository.GetByIdAsync(request.PetId, cancellationToken);

        if (pet is null)
            return new NotFoundError(nameof(Pet), request.PetId);

        if (pet.OwnerId != currentUserService.OwnerId)
            return new ForbiddenError();

        // Resolve blob names before RemoveImages removes them from the collection.
        var blobNames = pet.Images
            .Where(i => request.ImageIds.Contains(i.Id))
            .Select(i => i.BlobName)
            .ToList();

        // RemoveImages validates all IDs exist (throws DomainException if any are missing);
        // lift that into the railway as a DomainRuleError.
        var removal = DomainResult.Try(() => pet.RemoveImages(request.ImageIds));
        if (removal.IsFailure)
            return removal.Error;

        // Commit the DB removals first, then delete the blobs (see RemovePetImage for the
        // ordering rationale — fail toward orphaned blobs, never dangling rows).
        await petRepository.UpdateAsync(pet, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var blobName in blobNames)
            await blobStorageService.DeleteAsync(ContainerName, blobName, cancellationToken);

        return Unit.Value;
    }
}
