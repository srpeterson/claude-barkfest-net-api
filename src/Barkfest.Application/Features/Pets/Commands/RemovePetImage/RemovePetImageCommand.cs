using MediatR;

namespace Barkfest.Application.Features.Pets.Commands.RemovePetImage;

public record RemovePetImageCommand(Guid PetId, Guid ImageId) : IRequest;
