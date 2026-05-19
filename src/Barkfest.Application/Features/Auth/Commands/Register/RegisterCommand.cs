using MediatR;

namespace Barkfest.Application.Features.Auth.Commands.Register;

public record RegisterCommand(
    string Username,
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    string Password) : IRequest<Guid>;
