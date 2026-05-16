using Barkfest.Application.Common.Exceptions;
using Barkfest.Application.Common.Interfaces;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Interfaces;
using MediatR;

namespace Barkfest.Application.Features.Pets.Commands.AddPetImage;

public record AddPetImageCommand(
    Guid PetId,
    string FileName,
    Stream Content,
    string ContentType) : IRequest<Guid>;

public class AddPetImageCommandHandler(
    IPetRepository petRepository,
    IBlobStorageService blobStorageService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<AddPetImageCommand, Guid>
{
    private const string ContainerName = "pet-images";

    public async Task<Guid> Handle(AddPetImageCommand request, CancellationToken cancellationToken)
    {
        var pet = await petRepository.GetByIdAsync(request.PetId, cancellationToken);

        if (pet is null)
            throw new NotFoundException(nameof(Pet), request.PetId);

        var extension = Path.GetExtension(request.FileName);
        var blobName = $"pets/{request.PetId}/gallery/{Guid.CreateVersion7()}{extension}";

        await blobStorageService.UploadAsync(ContainerName, blobName, request.Content, request.ContentType, cancellationToken);

        var image = new PetImage();
        image.SetImage(blobName, request.ContentType);
        image.SetDisplayOrder(pet.Images.Count);

        pet.AddImage(image);

        await petRepository.UpdateAsync(pet, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return image.Id;
    }
}
