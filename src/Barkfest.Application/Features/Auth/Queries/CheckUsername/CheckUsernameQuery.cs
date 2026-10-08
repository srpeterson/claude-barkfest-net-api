using Barkfest.Domain.Errors;
using Barkfest.Domain.Interfaces;
using CSharpFunctionalExtensions;
using MediatR;

namespace Barkfest.Application.Features.Auth.Queries.CheckUsername;

public record CheckUsernameQuery(string Value) : IRequest<Result<bool, Error>>;

public class CheckUsernameQueryHandler(IOwnerRepository ownerRepository)
    : IRequestHandler<CheckUsernameQuery, Result<bool, Error>>
{
    public async Task<Result<bool, Error>> Handle(CheckUsernameQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Value))
            return true;

        return await ownerRepository.IsUsernameAvailableAsync(request.Value.Trim(), cancellationToken);
    }
}
