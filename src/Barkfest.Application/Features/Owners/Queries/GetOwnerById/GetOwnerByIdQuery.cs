using Barkfest.Application.Features.Owners.DTOs;
using MediatR;

namespace Barkfest.Application.Features.Owners.Queries.GetOwnerById;

public record GetOwnerByIdQuery(Guid Id) : IRequest<OwnerDto>;
