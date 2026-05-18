using Barkfest.Application.Common.Exceptions;
using Barkfest.Application.Common.Interfaces;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Exceptions;
using Barkfest.Domain.Interfaces;
using MediatR;

namespace Barkfest.Application.Features.Administrators.Commands.UpdateAdministratorPassword;

public record UpdateAdministratorPasswordCommand(Guid Id, string NewPassword) : IRequest;

public class UpdateAdministratorPasswordCommandHandler(
    IAdministratorRepository administratorRepository,
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    ICurrentUserService currentUserService) : IRequestHandler<UpdateAdministratorPasswordCommand>
{
    public async Task Handle(UpdateAdministratorPasswordCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAdmin)
            throw new ForbiddenException();

        var administrator = await administratorRepository.GetByIdAsync(request.Id, cancellationToken);

        if (administrator is null)
            throw new NotFoundException(nameof(Administrator), request.Id);

        administrator.SetPasswordHash(passwordHasher.Hash(request.NewPassword));

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
