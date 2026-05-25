using Barkfest.Application.Common.Exceptions;
using Barkfest.Application.Common.Interfaces;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Exceptions;
using Barkfest.Domain.Interfaces;
using MediatR;

namespace Barkfest.Application.Features.Pets.Commands.AddPetImages;

public record PetImageUpload(string FileName, Stream Content, string ContentType, long Length);

public record PetImageUploadResult(string FileName, bool Success, Guid? ImageId, string? FailureReason);

public record AddPetImagesResult(IReadOnlyList<PetImageUploadResult> Results);

public record AddPetImagesCommand(
    Guid PetId,
    IReadOnlyList<PetImageUpload> Images) : IRequest<AddPetImagesResult>;

public class AddPetImagesCommandHandler(
    IPetRepository petRepository,
    IBlobStorageService blobStorageService,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    IContentModerationService contentModerationService)
    : IRequestHandler<AddPetImagesCommand, AddPetImagesResult>
{
    private const string ContainerName = "pet-images";

    public async Task<AddPetImagesResult> Handle(AddPetImagesCommand request, CancellationToken cancellationToken)
    {
        var pet = await petRepository.GetByIdAsync(request.PetId, cancellationToken);

        if (pet is null)
            throw new NotFoundException(nameof(Pet), request.PetId);

        if (pet.OwnerId != currentUserService.OwnerId)
            throw new ForbiddenException();

        var availableSlots = Pet.MaxImages - pet.Images.Count;
        if (request.Images.Count > availableSlots)
            throw new DomainException(
                $"Only {availableSlots} slot(s) remaining. You submitted {request.Images.Count} images.");

        var results = new List<PetImageUploadResult>();

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

            var image = PetImage.Create(blobName, upload.ContentType, pet.Images.Count);
            pet.AddImage(image);

            results.Add(new PetImageUploadResult(upload.FileName, true, image.Id, null));
        }

        if (results.Any(r => r.Success))
        {
            await petRepository.UpdateAsync(pet, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return new AddPetImagesResult(results);
    }
}
