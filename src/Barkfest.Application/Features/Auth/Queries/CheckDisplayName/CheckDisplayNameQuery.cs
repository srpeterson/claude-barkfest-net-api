using Barkfest.Domain.Entities;
using Barkfest.Domain.Errors;
using Barkfest.Domain.Interfaces;
using CSharpFunctionalExtensions;
using MediatR;

namespace Barkfest.Application.Features.Auth.Queries.CheckDisplayName;

public record CheckDisplayNameQuery(string Value) : IRequest<Result<bool, Error>>;

public class CheckDisplayNameQueryHandler(IOwnerRepository ownerRepository)
    : IRequestHandler<CheckDisplayNameQuery, Result<bool, Error>>
{
    public async Task<Result<bool, Error>> Handle(CheckDisplayNameQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Value))
            return true;

        var normalized = Owner.Normalize(request.Value);
        return await ownerRepository.IsDisplayNameAvailableAsync(normalized, cancellationToken: cancellationToken);
    }
}
