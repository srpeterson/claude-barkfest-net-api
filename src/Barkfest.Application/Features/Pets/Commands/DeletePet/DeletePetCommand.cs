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

        foreach (var image in pet.Images)
            await blobStorageService.DeleteAsync(ContainerName, image.BlobName, cancellationToken);

        await petRepository.DeleteAsync(request.PetId, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
