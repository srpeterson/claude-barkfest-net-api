using MediatR;

namespace Barkfest.Application.Features.Pets.Commands.DeletePet;

public record DeletePetCommand(Guid Id) : IRequest;
