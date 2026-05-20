using Barkfest.Application.Common.Exceptions;
using Barkfest.Application.Common.Interfaces;
using Barkfest.Application.Features.Auth.DTOs;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Interfaces;
using MediatR;

namespace Barkfest.Application.Features.Auth.Commands.AdminLogin;

public class AdminLoginCommandHandler(
    IAdministratorRepository administratorRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService) : IRequestHandler<AdminLoginCommand, AuthTokenDto>
{
    public async Task<AuthTokenDto> Handle(AdminLoginCommand request, CancellationToken cancellationToken)
    {
        var administrator = await administratorRepository.GetByUsernameAsync(request.Username, cancellationToken);

        if (administrator is null)
            throw new NotFoundException(nameof(Administrator), "username", request.Username);

        if (!passwordHasher.Verify(request.Password, administrator.PasswordHash))
            throw new NotFoundException(nameof(Administrator), "username", request.Username);

        var token = jwtTokenService.GenerateAdminToken(administrator);
        var expiry = jwtTokenService.GetExpiry();

        return new AuthTokenDto(token, administrator.Id, expiry);
    }
}
