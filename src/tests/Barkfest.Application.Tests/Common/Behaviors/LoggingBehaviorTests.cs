using Barkfest.Application.Common.Behaviors;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Barkfest.Application.Tests.Common.Behaviors;

public class LoggingBehaviorTests
{
    private readonly ILogger<LoggingBehavior<TestRequest, string>> _logger =
        Substitute.For<ILogger<LoggingBehavior<TestRequest, string>>>();

    // NSubstitute cannot mock delegate types, so build a real next with a closure counter.
    private static (RequestHandlerDelegate<string> Delegate, Func<int> CallCount) MakeNext(string returns = "ok")
    {
        var count = 0;
        RequestHandlerDelegate<string> del = _ => { count++; return Task.FromResult(returns); };
        return (del, () => count);
    }

    [Fact]
    public async Task Handle_When_HandlerSucceeds_Returns_NextResult()
    {
        var (next, callCount) = MakeNext("expected");
        var loggingBehavior = new LoggingBehavior<TestRequest, string>(_logger);

        var result = await loggingBehavior.Handle(new TestRequest("hi"), next, CancellationToken.None);

        result.ShouldBe("expected");
        callCount().ShouldBe(1);
    }

    [Fact]
    public async Task Handle_When_HandlerThrows_Rethrows()
    {
        RequestHandlerDelegate<string> next = _ => throw new InvalidOperationException("boom");
        var loggingBehavior = new LoggingBehavior<TestRequest, string>(_logger);

        var act = async () => await loggingBehavior.Handle(new TestRequest("hi"), next, CancellationToken.None);

        await act.ShouldThrowAsync<InvalidOperationException>();
    }
}
