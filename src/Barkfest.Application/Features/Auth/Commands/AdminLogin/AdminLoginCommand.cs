using MediatR;
using Barkfest.Application.Features.Auth.DTOs;

namespace Barkfest.Application.Features.Auth.Commands.AdminLogin;

public record AdminLoginCommand(string Username, string Password) : IRequest<AuthTokenDto>;
