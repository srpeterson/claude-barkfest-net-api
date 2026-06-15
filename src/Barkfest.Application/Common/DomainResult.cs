using Barkfest.Domain.Errors;
using Barkfest.Domain.Exceptions;
using CSharpFunctionalExtensions;
using MediatR;

namespace Barkfest.Application.Common;

/// <summary>
/// The single sanctioned bridge between the exception-throwing domain (Depth A) and the
/// Result railway. Wraps a domain call that may throw <see cref="DomainException"/>,
/// converting it to a <see cref="DomainRuleError"/>. Any other exception propagates
/// (genuinely unexpected/infrastructural failures stay exceptions handled by middleware).
/// </summary>
public static class DomainResult
{
    public static Result<T, Error> Try<T>(Func<T> domainCall)
    {
        try
        {
            return domainCall();
        }
        catch (DomainException ex)
        {
            return new DomainRuleError(ex.Message);
        }
    }

    public static Result<Unit, Error> Try(Action domainCall)
    {
        try
        {
            domainCall();
            return Unit.Value;
        }
        catch (DomainException ex)
        {
            return new DomainRuleError(ex.Message);
        }
    }
}
