using Barkfest.Application.Common.Interfaces;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Errors;
using Barkfest.Domain.Interfaces;
using CSharpFunctionalExtensions;
using MediatR;

namespace Barkfest.Application.Features.Administrators.Commands.DeleteAdministrator;

public record DeleteAdministratorCommand(Guid AdministratorId) : IRequest<Result<Unit, Error>>;

public class DeleteAdministratorCommandHandler(
    IAdministratorRepository administratorRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService) : IRequestHandler<DeleteAdministratorCommand, Result<Unit, Error>>
{
    public async Task<Result<Unit, Error>> Handle(DeleteAdministratorCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAdmin)
            return new ForbiddenError();

        if (request.AdministratorId == currentUserService.AdminId)
            return new ForbiddenError();

        var administrator = await administratorRepository.GetByIdAsync(request.AdministratorId, cancellationToken);

        if (administrator is null)
            return new NotFoundError(nameof(Administrator), request.AdministratorId);

        await administratorRepository.DeleteAsync(administrator, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
