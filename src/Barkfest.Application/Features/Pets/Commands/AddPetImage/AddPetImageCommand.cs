using MediatR;

namespace Barkfest.Application.Features.Pets.Commands.AddPetImage;

public record AddPetImageCommand(
    Guid PetId,
    string FileName,
    Stream Content,
    string ContentType) : IRequest<Guid>;
