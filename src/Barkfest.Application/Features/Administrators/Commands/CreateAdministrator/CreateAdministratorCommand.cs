using Barkfest.Application.Common.Exceptions;
using Barkfest.Application.Common.Interfaces;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Exceptions;
using Barkfest.Domain.Interfaces;
using MediatR;

namespace Barkfest.Application.Features.Administrators.Commands.CreateAdministrator;

public record CreateAdministratorCommand(string Username, string Name, string Email, string PhoneNumber, string Password) : IRequest<Guid>;

public class CreateAdministratorCommandHandler(
    IAdministratorRepository administratorRepository,
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    ICurrentUserService currentUserService) : IRequestHandler<CreateAdministratorCommand, Guid>
{
    public async Task<Guid> Handle(CreateAdministratorCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAdmin)
            throw new ForbiddenException();

        var existingByUsername = await administratorRepository.GetByUsernameAsync(request.Username, cancellationToken);
        if (existingByUsername is not null)
            throw new DomainException($"An administrator with username '{request.Username.Trim()}' already exists.");

        var existingByEmail = await administratorRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existingByEmail is not null)
            throw new DomainException($"An administrator with email '{request.Email.Trim().ToLowerInvariant()}' already exists.");

        var administrator = new Administrator();
        administrator.SetUsername(request.Username);
        administrator.SetName(request.Name);
        administrator.SetEmail(request.Email);
        administrator.SetPhoneNumber(request.PhoneNumber);
        administrator.SetPasswordHash(passwordHasher.Hash(request.Password));

        await administratorRepository.AddAsync(administrator, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return administrator.Id;
    }
}
