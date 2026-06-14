using Barkfest.Application.Common;
using Barkfest.Application.Common.Interfaces;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Errors;
using Barkfest.Domain.Interfaces;
using CSharpFunctionalExtensions;
using MediatR;

namespace Barkfest.Application.Features.Pets.Commands.SetFeaturedImage;

public record SetFeaturedImageCommand(Guid PetId, Guid ImageId) : IRequest<Result<Unit, Error>>;

public class SetFeaturedImageCommandHandler(
    IPetRepository petRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService)
    : IRequestHandler<SetFeaturedImageCommand, Result<Unit, Error>>
{
    public async Task<Result<Unit, Error>> Handle(SetFeaturedImageCommand request, CancellationToken cancellationToken)
    {
        var pet = await petRepository.GetByIdAsync(request.PetId, cancellationToken);

        if (pet is null)
            return new NotFoundError(nameof(Pet), request.PetId);

        if (pet.OwnerId != currentUserService.OwnerId)
            return new ForbiddenError();

        // SetFeaturedImage throws DomainException if the image is not on the pet; lift it.
        var featured = DomainResult.Try(() => pet.SetFeaturedImage(request.ImageId));
        if (featured.IsFailure)
            return featured.Error;

        await petRepository.UpdateAsync(pet, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
