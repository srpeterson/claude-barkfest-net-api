using Barkfest.Application.Features.Pets.DTOs;
using MediatR;

namespace Barkfest.Application.Features.Pets.Queries.GetAllPets;

public record GetAllPetsQuery : IRequest<IEnumerable<PetDto>>;
