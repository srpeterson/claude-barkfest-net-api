using Barkfest.Domain.Interfaces;
using MediatR;

namespace Barkfest.Application.Features.Auth.Queries.CheckUsername;

public record CheckUsernameQuery(string Value) : IRequest<bool>;

public class CheckUsernameQueryHandler(IOwnerRepository ownerRepository)
    : IRequestHandler<CheckUsernameQuery, bool>
{
    public async Task<bool> Handle(CheckUsernameQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Value))
            return true;

        return await ownerRepository.IsUsernameAvailableAsync(request.Value.Trim(), cancellationToken);
    }
}
