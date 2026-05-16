using Barkfest.Application.Common.Exceptions;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Interfaces;
using MediatR;

namespace Barkfest.Application.Features.Pets.Commands.DeletePet;

public record DeletePetCommand(Guid Id) : IRequest;

public class DeletePetCommandHandler(IPetRepository petRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<DeletePetCommand>
{
    public async Task Handle(DeletePetCommand request, CancellationToken cancellationToken)
    {
        var pet = await petRepository.GetByIdAsync(request.Id, cancellationToken);

        if (pet is null)
            throw new NotFoundException(nameof(Pet), request.Id);

        await petRepository.DeleteAsync(request.Id, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
