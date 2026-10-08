using Barkfest.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Barkfest.API.Middleware;

// Backstop only. Expected failures (not found, forbidden, validation, domain-rule
// violations) flow through the Result railway and are translated by ResultExtensions.
// This middleware handles what the railway does not: a DomainException that escapes the
// DomainResult.Try bridge, and any otherwise-unhandled exception (500).
public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (DomainException ex)
        {
            logger.LogWarning(ex, "Domain rule violated");
            await WriteProblem(context, StatusCodes.Status400BadRequest, "Bad Request", ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception");
            await WriteProblem(context, StatusCodes.Status500InternalServerError, "Internal Server Error", "An unexpected error occurred.");
        }
    }

    private static async Task WriteProblem(HttpContext context, int status, string title, string detail)
    {
        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(new ProblemDetails { Status = status, Title = title, Detail = detail });
    }
}
