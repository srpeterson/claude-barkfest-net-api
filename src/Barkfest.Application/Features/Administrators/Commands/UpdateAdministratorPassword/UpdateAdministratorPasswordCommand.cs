using Barkfest.Application.Common.Interfaces;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Errors;
using Barkfest.Domain.Interfaces;
using CSharpFunctionalExtensions;
using MediatR;

namespace Barkfest.Application.Features.Administrators.Commands.UpdateAdministratorPassword;

public record UpdateAdministratorPasswordCommand(Guid AdministratorId, string NewPassword) : IRequest<Result<Unit, Error>>;

public class UpdateAdministratorPasswordCommandHandler(
    IAdministratorRepository administratorRepository,
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    ICurrentUserService currentUserService) : IRequestHandler<UpdateAdministratorPasswordCommand, Result<Unit, Error>>
{
    public async Task<Result<Unit, Error>> Handle(UpdateAdministratorPasswordCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAdmin)
            return new ForbiddenError();

        var administrator = await administratorRepository.GetByIdAsync(request.AdministratorId, cancellationToken);

        if (administrator is null)
            return new NotFoundError(nameof(Administrator), request.AdministratorId);

        administrator.SetPasswordHash(passwordHasher.Hash(request.NewPassword));

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
