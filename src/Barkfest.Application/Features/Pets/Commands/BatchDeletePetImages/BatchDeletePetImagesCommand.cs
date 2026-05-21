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
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService)
    : IRequestHandler<BatchDeletePetImagesCommand>
{
    public async Task Handle(BatchDeletePetImagesCommand request, CancellationToken cancellationToken)
    {
        var pet = await petRepository.GetByIdAsync(request.PetId, cancellationToken);

        if (pet is null)
            throw new NotFoundException(nameof(Pet), request.PetId);

        if (pet.OwnerId != currentUserService.OwnerId)
            throw new ForbiddenException();

        pet.RemoveImages(request.ImageIds);

        await petRepository.UpdateAsync(pet, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
