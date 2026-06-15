using Barkfest.Application.Common;
using Barkfest.Application.Common.Interfaces;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Errors;
using Barkfest.Domain.Interfaces;
using CSharpFunctionalExtensions;
using MediatR;

namespace Barkfest.Application.Features.Administrators.Commands.CreateAdministrator;

public record CreateAdministratorCommand(string Username, string Name, string Email, string PhoneNumber, string Password) : IRequest<Result<Guid, Error>>;

public class CreateAdministratorCommandHandler(
    IAdministratorRepository administratorRepository,
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    ICurrentUserService currentUserService) : IRequestHandler<CreateAdministratorCommand, Result<Guid, Error>>
{
    public async Task<Result<Guid, Error>> Handle(CreateAdministratorCommand request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAdmin)
            return new ForbiddenError();

        var existingByUsername = await administratorRepository.GetByUsernameAsync(request.Username, cancellationToken);
        if (existingByUsername is not null)
            return new DomainRuleError($"An administrator with username '{request.Username.Trim()}' already exists.");

        var existingByEmail = await administratorRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existingByEmail is not null)
            return new DomainRuleError($"An administrator with email '{request.Email.Trim().ToLowerInvariant()}' already exists.");

        var creation = DomainResult.Try(() => Administrator.Create(
            request.Username,
            request.Name,
            request.Email,
            request.PhoneNumber,
            passwordHasher.Hash(request.Password)));

        if (creation.IsFailure)
            return creation.Error;

        await administratorRepository.AddAsync(creation.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return creation.Value.Id;
    }
}
