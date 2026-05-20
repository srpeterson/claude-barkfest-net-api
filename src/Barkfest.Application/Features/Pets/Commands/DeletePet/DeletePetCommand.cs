using Barkfest.Application.Common.Exceptions;
using Barkfest.Application.Common.Interfaces;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Exceptions;
using Barkfest.Domain.Interfaces;
using MediatR;

namespace Barkfest.Application.Features.Pets.Commands.DeletePet;

public record DeletePetCommand(Guid Id) : IRequest;

public class DeletePetCommandHandler(IPetRepository petRepository, IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    : IRequestHandler<DeletePetCommand>
{
    public async Task Handle(DeletePetCommand request, CancellationToken cancellationToken)
    {
        var pet = await petRepository.GetByIdAsync(request.Id, cancellationToken);

        if (pet is null)
            throw new NotFoundException(nameof(Pet), request.Id);

        if (pet.OwnerId != currentUserService.OwnerId)
            throw new ForbiddenException();

        await petRepository.DeleteAsync(request.Id, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
