using MediatR;

namespace Barkfest.Application.Features.Auth.Commands.Register;

public record RegisterCommand(
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    string Password) : IRequest<Guid>;
