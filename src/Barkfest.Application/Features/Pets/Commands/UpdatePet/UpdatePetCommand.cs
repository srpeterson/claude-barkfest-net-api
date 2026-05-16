using MediatR;

namespace Barkfest.Application.Features.Pets.Commands.UpdatePet;

public record UpdatePetCommand(
    Guid Id,
    string Name,
    string? Description,
    DateOnly? DateOfBirth,
    string PetType) : IRequest;
