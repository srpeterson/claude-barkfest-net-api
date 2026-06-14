using Barkfest.Application.Common.Interfaces;
using Barkfest.Application.Features.Auth.DTOs;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Errors;
using Barkfest.Domain.Interfaces;
using CSharpFunctionalExtensions;
using MediatR;

namespace Barkfest.Application.Features.Auth.Commands.Login;

public record LoginCommand(string Username, string Password) : IRequest<Result<AuthTokenDto, Error>>;

public class LoginCommandHandler(
    IOwnerRepository ownerRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService) : IRequestHandler<LoginCommand, Result<AuthTokenDto, Error>>
{
    public async Task<Result<AuthTokenDto, Error>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var owner = await ownerRepository.GetByUsernameAsync(request.Username, cancellationToken);
        if (owner is null)
            return new NotFoundError(nameof(Owner), request.Username, "username");

        if (!passwordHasher.Verify(request.Password, owner.PasswordHash))
            return new NotFoundError(nameof(Owner), request.Username, "username");

        if (!owner.IsActive)
            return new ForbiddenError();

        var token = jwtTokenService.GenerateOwnerToken(owner);
        var expiry = jwtTokenService.GetExpiry();

        return new AuthTokenDto(token, owner.Id, expiry);
    }
}
