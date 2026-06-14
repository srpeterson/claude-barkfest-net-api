using Barkfest.Application.Common.Interfaces;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Errors;
using Barkfest.Domain.Interfaces;
using CSharpFunctionalExtensions;
using MediatR;

namespace Barkfest.Application.Features.Pets.Commands.DeletePet;

public record DeletePetCommand(Guid PetId) : IRequest<Result<Unit, Error>>;

public class DeletePetCommandHandler(
    IPetRepository petRepository,
    IBlobStorageService blobStorageService,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService)
    : IRequestHandler<DeletePetCommand, Result<Unit, Error>>
{
    private const string ContainerName = "pet-images";

    public async Task<Result<Unit, Error>> Handle(DeletePetCommand request, CancellationToken cancellationToken)
    {
        var pet = await petRepository.GetByIdAsync(request.PetId, cancellationToken);

        if (pet is null)
            return new NotFoundError(nameof(Pet), request.PetId);

        if (pet.OwnerId != currentUserService.OwnerId)
            return new ForbiddenError();

        // Capture blob names before deletion, then commit the DB delete (cascade removes the
        // PetImages rows) before deleting blobs — fail toward orphaned blobs, never rows that
        // point at deleted blobs (see RemovePetImage for the rationale).
        var blobNames = pet.Images.Select(i => i.BlobName).ToList();

        await petRepository.DeleteAsync(request.PetId, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var blobName in blobNames)
            await blobStorageService.DeleteAsync(ContainerName, blobName, cancellationToken);

        return Unit.Value;
    }
}
