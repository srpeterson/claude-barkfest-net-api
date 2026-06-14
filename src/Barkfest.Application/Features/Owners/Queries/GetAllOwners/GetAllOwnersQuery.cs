using Barkfest.Application.Common.Interfaces;
using Barkfest.Application.Features.Owners.DTOs;
using Barkfest.Domain.Errors;
using Barkfest.Domain.Interfaces;
using CSharpFunctionalExtensions;
using MediatR;

namespace Barkfest.Application.Features.Owners.Queries.GetAllOwners;

public record GetAllOwnersQuery : IRequest<Result<IEnumerable<OwnerDto>, Error>>;

public class GetAllOwnersQueryHandler(
    IOwnerRepository ownerRepository,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetAllOwnersQuery, Result<IEnumerable<OwnerDto>, Error>>
{
    public async Task<Result<IEnumerable<OwnerDto>, Error>> Handle(GetAllOwnersQuery request, CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAdmin)
            return new ForbiddenError();

        var owners = await ownerRepository.GetAllAsync(cancellationToken);
        return Result.Success<IEnumerable<OwnerDto>, Error>(owners.ToDtoList());
    }
}
