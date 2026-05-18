using Barkfest.Application.Common.Exceptions;
using Barkfest.Application.Common.Interfaces;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Exceptions;
using Barkfest.Domain.Interfaces;
using MediatR;

namespace Barkfest.Application.Features.Administrators.Commands.DeleteAdministrator;

public record DeleteAdministratorCommand(Guid Id) : IRequest;

public class DeleteAdministratorCommandHandler(
    IAdministratorRepository administratorRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService) : IRequestHandler<DeleteAdministratorCommand>
{
    public async Task Handle(DeleteAdministratorCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAdmin)
            throw new ForbiddenException();

        if (request.Id == currentUserService.AdminId)
            throw new ForbiddenException();

        var administrator = await administratorRepository.GetByIdAsync(request.Id, cancellationToken);

        if (administrator is null)
            throw new NotFoundException(nameof(Administrator), request.Id);

        await administratorRepository.DeleteAsync(administrator, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
