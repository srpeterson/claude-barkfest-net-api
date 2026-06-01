using Barkfest.Application.Common.Exceptions;
using Barkfest.Application.Common.Interfaces;
using Barkfest.Application.Features.Owners.DTOs;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Interfaces;
using MediatR;

namespace Barkfest.Application.Features.Owners.Queries.GetOwnerById;

public record GetOwnerByIdQuery(Guid Id) : IRequest<OwnerDto>;

public class GetOwnerByIdQueryHandler(IOwnerRepository ownerRepository, ICurrentUserService currentUserService)
    : IRequestHandler<GetOwnerByIdQuery, OwnerDto>
{
    public async Task<OwnerDto> Handle(GetOwnerByIdQuery request, CancellationToken cancellationToken)
    {
        var owner = await ownerRepository.GetByIdAsync(request.Id, cancellationToken);

        if (owner is null)
            throw new NotFoundException(nameof(Owner), request.Id);

        // IsActive is admin-controlled — inactive owners are hidden from everyone except admins.
        if (!owner.IsActive && !currentUserService.IsAdmin)
            throw new NotFoundException(nameof(Owner), request.Id);

        // IsVisible is owner-controlled — hidden from others, but the owner can always see their own profile.
        if (!owner.IsVisible && !currentUserService.IsAdmin && currentUserService.OwnerId != owner.Id)
            throw new NotFoundException(nameof(Owner), request.Id);

        return owner.ToDto();
    }
}
