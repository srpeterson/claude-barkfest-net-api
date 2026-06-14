using Barkfest.Application.Common.Interfaces;
using Barkfest.Application.Features.Owners.DTOs;
using Barkfest.Domain.Entities;
using Barkfest.Domain.Errors;
using Barkfest.Domain.Interfaces;
using CSharpFunctionalExtensions;
using MediatR;

namespace Barkfest.Application.Features.Owners.Queries.GetOwnerById;

public record GetOwnerByIdQuery(Guid OwnerId) : IRequest<Result<OwnerDto, Error>>;

public class GetOwnerByIdQueryHandler(IOwnerRepository ownerRepository, ICurrentUserService currentUserService)
    : IRequestHandler<GetOwnerByIdQuery, Result<OwnerDto, Error>>
{
    public async Task<Result<OwnerDto, Error>> Handle(GetOwnerByIdQuery request, CancellationToken cancellationToken)
    {
        var owner = await ownerRepository.GetByIdAsync(request.OwnerId, cancellationToken);

        if (owner is null)
            return new NotFoundError(nameof(Owner), request.OwnerId);

        // IsActive is admin-controlled — inactive owners are hidden from everyone except admins.
        if (!owner.IsActive && !currentUserService.IsAdmin)
            return new NotFoundError(nameof(Owner), request.OwnerId);

        // IsVisible is owner-controlled — hidden from others, but the owner can always see their own profile.
        if (!owner.IsVisible && !currentUserService.IsAdmin && currentUserService.OwnerId != owner.Id)
            return new NotFoundError(nameof(Owner), request.OwnerId);

        return owner.ToDto();
    }
}
