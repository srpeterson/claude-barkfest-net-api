using MediatR;

namespace Barkfest.Application.Features.Owners.Commands.UploadOwnerProfileImage;

public record UploadOwnerProfileImageCommand(
    Guid OwnerId,
    string FileName,
    Stream Content,
    string ContentType) : IRequest;
