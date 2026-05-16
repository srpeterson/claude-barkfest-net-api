using MediatR;

namespace Barkfest.Application.Features.Owners.Commands.UpdateOwner;

public record UpdateOwnerCommand(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber) : IRequest;
