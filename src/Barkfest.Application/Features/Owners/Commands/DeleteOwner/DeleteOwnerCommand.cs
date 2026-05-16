using MediatR;

namespace Barkfest.Application.Features.Owners.Commands.DeleteOwner;

public record DeleteOwnerCommand(Guid Id) : IRequest;
