using Barkfest.Application.Common.Interfaces;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Errors;
using Barkfest.Domain.Interfaces;
using CSharpFunctionalExtensions;
using MediatR;

namespace Barkfest.Application.Features.Pets.Commands.RemovePetImage;

public record RemovePetImageCommand(Guid PetId, Guid ImageId) : IRequest<Result<Unit, Error>>;

public class RemovePetImageCommandHandler(
    IPetRepository petRepository,
    IBlobStorageService blobStorageService,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService)
    : IRequestHandler<RemovePetImageCommand, Result<Unit, Error>>
{
    private const string ContainerName = "pet-images";

    public async Task<Result<Unit, Error>> Handle(RemovePetImageCommand request, CancellationToken cancellationToken)
    {
        var pet = await petRepository.GetByIdAsync(request.PetId, cancellationToken);

        if (pet is null)
            return new NotFoundError(nameof(Pet), request.PetId);

        if (pet.OwnerId != currentUserService.OwnerId && !currentUserService.IsAdmin)
            return new ForbiddenError();

        var image = pet.Images.FirstOrDefault(i => i.Id == request.ImageId);

        if (image is null)
            return new NotFoundError(nameof(PetImage), request.ImageId);

        var blobName = image.BlobName;
        pet.RemoveImage(request.ImageId);

        // Commit the DB removal first, then delete the blob. If the save fails the blob is
        // untouched (clean no-op); if the blob delete fails afterwards we leak an orphaned
        // blob (safe, reclaimable) rather than leaving a row pointing at a deleted blob.
        await petRepository.UpdateAsync(pet, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await blobStorageService.DeleteAsync(ContainerName, blobName, cancellationToken);

        return Unit.Value;
    }
}
