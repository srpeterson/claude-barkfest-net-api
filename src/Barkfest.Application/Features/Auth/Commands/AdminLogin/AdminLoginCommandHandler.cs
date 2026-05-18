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
        var administrator = await administratorRepository.GetByEmailAsync(request.Email, cancellationToken);

        if (administrator is null)
            throw new NotFoundException(nameof(Administrator), request.Email);

        if (!passwordHasher.Verify(request.Password, administrator.PasswordHash))
            throw new NotFoundException(nameof(Administrator), request.Email);

        var token = jwtTokenService.GenerateAdminToken(administrator);
        var expiry = jwtTokenService.GetExpiry();

        return new AuthTokenDto(token, administrator.Id, expiry);
    }
}
