using Barkfest.Application.Features.Pets.DTOs;
using MediatR;

namespace Barkfest.Application.Features.Pets.Queries.GetPetById;

public record GetPetByIdQuery(Guid Id) : IRequest<PetDto>;
