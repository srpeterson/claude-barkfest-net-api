using Barkfest.Application.Common.Interfaces;
using Barkfest.Application.Features.Auth.DTOs;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Errors;
using Barkfest.Domain.Interfaces;
using CSharpFunctionalExtensions;
using MediatR;

namespace Barkfest.Application.Features.Auth.Commands.AdminLogin;

public record AdminLoginCommand(string Username, string Password) : IRequest<Result<AuthTokenDto, Error>>;

public class AdminLoginCommandHandler(
    IAdministratorRepository administratorRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService) : IRequestHandler<AdminLoginCommand, Result<AuthTokenDto, Error>>
{
    public async Task<Result<AuthTokenDto, Error>> Handle(AdminLoginCommand request, CancellationToken cancellationToken)
    {
        var administrator = await administratorRepository.GetByUsernameAsync(request.Username, cancellationToken);

        if (administrator is null)
            return new NotFoundError(nameof(Administrator), request.Username, "username");

        if (!passwordHasher.Verify(request.Password, administrator.PasswordHash))
            return new NotFoundError(nameof(Administrator), request.Username, "username");

        var token = jwtTokenService.GenerateAdminToken(administrator);
        var expiry = jwtTokenService.GetExpiry();

        return new AuthTokenDto(token, administrator.Id, expiry);
    }
}
