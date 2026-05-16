using MediatR;

namespace Barkfest.Application.Features.Owners.Commands.CreateOwner;

public record CreateOwnerCommand(
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber) : IRequest<Guid>;
