using Barkfest.Domain.Interfaces;
using MediatR;

namespace Barkfest.Application.Features.Auth.Queries.CheckDisplayName;

public record CheckDisplayNameQuery(string Value) : IRequest<bool>;

public class CheckDisplayNameQueryHandler(IOwnerRepository ownerRepository)
    : IRequestHandler<CheckDisplayNameQuery, bool>
{
    public async Task<bool> Handle(CheckDisplayNameQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Value))
            return true;

        var normalized = request.Value.Replace(" ", "").ToLowerInvariant();
        return await ownerRepository.IsDisplayNameAvailableAsync(normalized, cancellationToken);
    }
}
