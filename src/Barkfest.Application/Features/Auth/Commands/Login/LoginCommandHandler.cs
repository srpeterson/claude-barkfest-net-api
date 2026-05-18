using Barkfest.Application.Common.Exceptions;
using Barkfest.Application.Common.Interfaces;
using Barkfest.Application.Features.Auth.DTOs;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Exceptions;
using Barkfest.Domain.Interfaces;
using MediatR;

namespace Barkfest.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler(
    IOwnerRepository ownerRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService) : IRequestHandler<LoginCommand, AuthTokenDto>
{
    public async Task<AuthTokenDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var owner = await ownerRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (owner is null)
            throw new NotFoundException(nameof(Owner), request.Email);

        if (!passwordHasher.Verify(request.Password, owner.PasswordHash))
            throw new NotFoundException(nameof(Owner), request.Email);

        if (!owner.Active)
            throw new ForbiddenException();

        var token = jwtTokenService.GenerateOwnerToken(owner);
        var expiry = jwtTokenService.GetExpiry();

        return new AuthTokenDto(token, owner.Id, expiry);
    }
}
