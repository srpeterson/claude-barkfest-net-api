using Barkfest.Domain.Errors;
using FluentValidation;
using MediatR;

namespace Barkfest.Application.Common.Behaviors;

public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!validators.Any())
            return await next(cancellationToken);

        var context = new ValidationContext<TRequest>(request);

        var failures = validators
            .Select(v => v.Validate(context))
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count == 0)
            return await next(cancellationToken);

        // Result-returning handlers get a failed Result<T, Error> so validation flows
        // through the railway. Handlers not yet migrated to Result still throw
        // ValidationException (handled by middleware) - this legacy branch is removed
        // once the migration sweep is complete and every handler returns Result.
        if (ResultFailureFactory.IsResult<TResponse>())
        {
            var validationError = new ValidationError(
                failures
                    .GroupBy(f => f.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(f => f.ErrorMessage).ToArray()));

            return ResultFailureFactory.Create<TResponse>(validationError);
        }

        throw new ValidationException(failures);
    }
}
