using Barkfest.Domain.Errors;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;

namespace Barkfest.API.Extensions;

/// <summary>
/// Translates a <see cref="Result{T, Error}"/> into an <see cref="IActionResult"/> at the
/// API boundary. Success shapes are chosen by the caller (Ok / Created / NoContent);
/// failures map by <see cref="Error"/> case to the same ProblemDetails responses the
/// ExceptionHandlingMiddleware produced, preserving external behaviour.
/// </summary>
public static class ResultExtensions
{
    /// <summary>Success → 200 Ok with the value; failure → mapped ProblemDetails.</summary>
    public static IActionResult ToActionResult<T>(this Result<T, Error> result) =>
        result.IsSuccess ? new OkObjectResult(result.Value) : Problem(result.Error);

    /// <summary>Success → caller-supplied result (e.g. CreatedAtAction); failure → mapped ProblemDetails.</summary>
    public static IActionResult ToActionResult<T>(this Result<T, Error> result, Func<T, IActionResult> onSuccess) =>
        result.IsSuccess ? onSuccess(result.Value) : Problem(result.Error);

    /// <summary>Success → 204 NoContent (value ignored); failure → mapped ProblemDetails.</summary>
    public static IActionResult ToNoContentResult<T>(this Result<T, Error> result) =>
        result.IsSuccess ? new NoContentResult() : Problem(result.Error);

    private static ObjectResult Problem(Error error)
    {
        var (status, details) = error switch
        {
            NotFoundError e => (
                StatusCodes.Status404NotFound,
                (ProblemDetails)new ProblemDetails { Status = 404, Title = "Not Found", Detail = FormatNotFound(e) }),
            ForbiddenError e => (
                StatusCodes.Status403Forbidden,
                new ProblemDetails { Status = 403, Title = "Forbidden", Detail = e.Message ?? "Access denied." }),
            DomainRuleError e => (
                StatusCodes.Status400BadRequest,
                new ProblemDetails { Status = 400, Title = "Bad Request", Detail = e.Message }),
            ValidationError e => (
                StatusCodes.Status400BadRequest,
                new ValidationProblemDetails(e.Failures.ToDictionary(k => k.Key, v => v.Value))
                {
                    Status = 400,
                    Title = "Validation failed"
                }),
            _ => throw new InvalidOperationException(
                $"Unmapped error type '{error.GetType().Name}'. Add it to ResultExtensions.Problem.")
        };

        return new ObjectResult(details)
        {
            StatusCode = status,
            ContentTypes = { "application/problem+json" }
        };
    }

    private static string FormatNotFound(NotFoundError e) =>
        e.Field is null
            ? $"{e.Entity} with id '{e.Key}' was not found."
            : $"{e.Entity} with {e.Field} '{e.Key}' was not found.";
}
