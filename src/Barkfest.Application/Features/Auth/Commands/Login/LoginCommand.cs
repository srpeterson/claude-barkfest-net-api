using Barkfest.Application.Features.Auth.DTOs;
using MediatR;

namespace Barkfest.Application.Features.Auth.Commands.Login;

public record LoginCommand(string Email, string Password) : IRequest<AuthTokenDto>;
