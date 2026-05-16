using Barkfest.Application.Features.Pets.DTOs;
using MediatR;

namespace Barkfest.Application.Features.Pets.Queries.GetPetsByOwnerId;

public record GetPetsByOwnerIdQuery(Guid OwnerId) : IRequest<IEnumerable<PetDto>>;
