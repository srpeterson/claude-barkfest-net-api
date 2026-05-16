using Barkfest.Application.Common.Exceptions;
using Barkfest.Application.Features.Owners.DTOs;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Interfaces;
using MediatR;

namespace Barkfest.Application.Features.Owners.Queries.GetOwnerById;

public record GetOwnerByIdQuery(Guid Id) : IRequest<OwnerDto>;

public class GetOwnerByIdQueryHandler(IOwnerRepository ownerRepository)
    : IRequestHandler<GetOwnerByIdQuery, OwnerDto>
{
    public async Task<OwnerDto> Handle(GetOwnerByIdQuery request, CancellationToken cancellationToken)
    {
        var owner = await ownerRepository.GetByIdAsync(request.Id, cancellationToken);

        if (owner is null)
            throw new NotFoundException(nameof(Owner), request.Id);

        return owner.ToDto();
    }
}
