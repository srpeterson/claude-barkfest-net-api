using Barkfest.Application.Common.Exceptions;
using Barkfest.Application.Common.Interfaces;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Exceptions;
using Barkfest.Domain.Interfaces;
using MediatR;

namespace Barkfest.Application.Features.Pets.Commands.SetFeaturedImage;

public record SetFeaturedImageCommand(Guid PetId, Guid ImageId) : IRequest;

public class SetFeaturedImageCommandHandler(
    IPetRepository petRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService)
    : IRequestHandler<SetFeaturedImageCommand>
{
    public async Task Handle(SetFeaturedImageCommand request, CancellationToken cancellationToken)
    {
        var pet = await petRepository.GetByIdAsync(request.PetId, cancellationToken);

        if (pet is null)
            throw new NotFoundException(nameof(Pet), request.PetId);

        if (pet.OwnerId != currentUserService.OwnerId)
            throw new ForbiddenException();

        pet.SetFeaturedImage(request.ImageId);

        await petRepository.UpdateAsync(pet, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
