using Barkfest.Application.Features.Owners.DTOs;
using MediatR;

namespace Barkfest.Application.Features.Owners.Queries.GetAllOwners;

public record GetAllOwnersQuery : IRequest<IEnumerable<OwnerDto>>;
