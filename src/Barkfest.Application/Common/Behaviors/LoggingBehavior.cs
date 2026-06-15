using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Barkfest.Application.Common.Behaviors;

public class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        logger.LogDebug("Handling {RequestName}", requestName);

        var stopwatch = Stopwatch.StartNew();
        try
        {
            var response = await next(cancellationToken);
            stopwatch.Stop();
            logger.LogInformation("Handled {RequestName} in {ElapsedMilliseconds}ms", requestName, stopwatch.ElapsedMilliseconds);
            return response;
        }
        catch (Exception ex)
        {
            // Only genuine exceptions reach here - expected failures flow back as a failed
            // Result<T, Error> (a successful TResponse), so they are not logged as errors.
            stopwatch.Stop();
            logger.LogError(ex, "{RequestName} threw after {ElapsedMilliseconds}ms", requestName, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}
