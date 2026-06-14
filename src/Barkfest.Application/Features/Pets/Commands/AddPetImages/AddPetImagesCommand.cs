using Barkfest.Application.Common.Interfaces;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Errors;
using Barkfest.Domain.Interfaces;
using CSharpFunctionalExtensions;
using MediatR;

namespace Barkfest.Application.Features.Pets.Commands.AddPetImages;

public record PetImageUpload(string FileName, Stream Content, string ContentType, long Length);

public record PetImageUploadResult(string FileName, bool Success, Guid? ImageId, string? FailureReason);

public record AddPetImagesResult(IReadOnlyList<PetImageUploadResult> Results);

public record AddPetImagesCommand(
    Guid PetId,
    IReadOnlyList<PetImageUpload> Images) : IRequest<Result<AddPetImagesResult, Error>>;

public class AddPetImagesCommandHandler(
    IPetRepository petRepository,
    IBlobStorageService blobStorageService,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    IContentModerationService contentModerationService)
    : IRequestHandler<AddPetImagesCommand, Result<AddPetImagesResult, Error>>
{
    private const string ContainerName = "pet-images";

    public async Task<Result<AddPetImagesResult, Error>> Handle(AddPetImagesCommand request, CancellationToken cancellationToken)
    {
        var pet = await petRepository.GetByIdAsync(request.PetId, cancellationToken);

        if (pet is null)
            return new NotFoundError(nameof(Pet), request.PetId);

        if (pet.OwnerId != currentUserService.OwnerId)
            return new ForbiddenError();

        var availableSlots = Pet.MaxImages - pet.Images.Count;
        if (request.Images.Count > availableSlots)
            return new DomainRuleError(
                $"Only {availableSlots} slot(s) remaining. You submitted {request.Images.Count} images.");

        var results = new List<PetImageUploadResult>();
        var uploadedBlobs = new List<string>();

        foreach (var upload in request.Images)
        {
            var isSafe = await contentModerationService.IsImageSafeAsync(upload.Content, cancellationToken);
            if (!isSafe)
            {
                results.Add(new PetImageUploadResult(upload.FileName, false, null,
                    "Image was rejected by content moderation."));
                continue;
            }

            var extension = Path.GetExtension(upload.FileName);
            var blobName = $"pets/{request.PetId}/gallery/{Guid.CreateVersion7()}{extension}";

            await blobStorageService.UploadAsync(
                ContainerName, blobName, upload.Content, upload.ContentType, cancellationToken);
            uploadedBlobs.Add(blobName);

            var image = PetImage.Create(blobName, upload.ContentType, pet.Images.Count);
            pet.AddImage(image);

            results.Add(new PetImageUploadResult(upload.FileName, true, image.Id, null));
        }

        if (results.Any(r => r.Success))
        {
            try
            {
                await petRepository.UpdateAsync(pet, cancellationToken);
                await unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch
            {
                // The blobs were uploaded before the DB commit. If the save fails, delete them
                // so we fail toward an (invisible, reclaimable) orphaned blob rather than a row
                // that never got written. Best-effort; the exception still propagates (500).
                foreach (var blobName in uploadedBlobs)
                    await blobStorageService.DeleteAsync(ContainerName, blobName, CancellationToken.None);
                throw;
            }
        }

        return new AddPetImagesResult(results);
    }
}
