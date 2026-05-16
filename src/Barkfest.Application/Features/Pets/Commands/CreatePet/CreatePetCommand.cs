using MediatR;

namespace Barkfest.Application.Features.Pets.Commands.CreatePet;

public record CreatePetCommand(
    Guid OwnerId,
    string Name,
    string? Description,
    DateOnly? DateOfBirth,
    string PetType) : IRequest<Guid>;
