using MediatR;

namespace Barkfest.Application.Features.Pets.Commands.RemovePetProfileImage;

public record RemovePetProfileImageCommand(Guid PetId) : IRequest;
