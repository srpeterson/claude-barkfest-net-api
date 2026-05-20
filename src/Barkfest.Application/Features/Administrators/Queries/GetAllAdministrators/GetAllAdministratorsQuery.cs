using Barkfest.Application.Common.Interfaces;
using Barkfest.Application.Features.Administrators.DTOs;
using Barkfest.Domain.Exceptions;
using Barkfest.Domain.Interfaces;
using MediatR;

namespace Barkfest.Application.Features.Administrators.Queries.GetAllAdministrators;

public record GetAllAdministratorsQuery : IRequest<IEnumerable<AdministratorDto>>;

public class GetAllAdministratorsQueryHandler(
    IAdministratorRepository administratorRepository,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetAllAdministratorsQuery, IEnumerable<AdministratorDto>>
{
    public async Task<IEnumerable<AdministratorDto>> Handle(
        GetAllAdministratorsQuery request,
        CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAdmin)
            throw new ForbiddenException();

        var administrators = await administratorRepository.GetAllAsync(cancellationToken);
        return administrators.ToDtoList();
    }
}
