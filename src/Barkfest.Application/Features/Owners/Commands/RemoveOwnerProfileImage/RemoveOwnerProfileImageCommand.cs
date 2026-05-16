using MediatR;

namespace Barkfest.Application.Features.Owners.Commands.RemoveOwnerProfileImage;

public record RemoveOwnerProfileImageCommand(Guid OwnerId) : IRequest;
