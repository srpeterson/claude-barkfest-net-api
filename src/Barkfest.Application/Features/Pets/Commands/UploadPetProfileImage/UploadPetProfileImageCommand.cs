using MediatR;

namespace Barkfest.Application.Features.Pets.Commands.UploadPetProfileImage;

public record UploadPetProfileImageCommand(
    Guid PetId,
    string FileName,
    Stream Content,
    string ContentType) : IRequest;
