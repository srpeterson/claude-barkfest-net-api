using Barkfest.Application.Common.Interfaces;
using Barkfest.Application.Features.Administrators.DTOs;
using Barkfest.Domain.Errors;
using Barkfest.Domain.Interfaces;
using CSharpFunctionalExtensions;
using MediatR;

namespace Barkfest.Application.Features.Administrators.Queries.GetAllAdministrators;

public record GetAllAdministratorsQuery : IRequest<Result<IEnumerable<AdministratorDto>, Error>>;

public class GetAllAdministratorsQueryHandler(
    IAdministratorRepository administratorRepository,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetAllAdministratorsQuery, Result<IEnumerable<AdministratorDto>, Error>>
{
    public async Task<Result<IEnumerable<AdministratorDto>, Error>> Handle(
        GetAllAdministratorsQuery request,
        CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAdmin)
            return new ForbiddenError();

        var administrators = await administratorRepository.GetAllAsync(cancellationToken);
        return Result.Success<IEnumerable<AdministratorDto>, Error>(administrators.ToDtoList());
    }
}
