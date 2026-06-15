using Barkfest.Application.Common.Behaviors;
using Barkfest.Domain.Errors;
using CSharpFunctionalExtensions;
using FluentValidation;
using MediatR;

namespace Barkfest.Application.Tests.Common.Behaviors;

// Must be top-level (not nested) so NSubstitute / Castle DynamicProxy can
// create a proxy for IValidator<TestRequest> against a strong-named assembly.
public record TestRequest(string Value) : IRequest<string>;

public class TestRequestAlwaysPassValidator : AbstractValidator<TestRequest> { }

public class TestRequestAlwaysFailValidator : AbstractValidator<TestRequest>
{
    public TestRequestAlwaysFailValidator(string message = "Error.") =>
        RuleFor(x => x.Value).Must(_ => false).WithMessage(message);
}

// Result-returning request to exercise the railway branch of the behavior.
public record TestResultRequest(string Value) : IRequest<Result<string, Error>>;

public class TestResultRequestAlwaysPassValidator : AbstractValidator<TestResultRequest> { }

public class TestResultRequestAlwaysFailValidator : AbstractValidator<TestResultRequest>
{
    public TestResultRequestAlwaysFailValidator() =>
        RuleFor(x => x.Value).Must(_ => false).WithMessage("Bad value.");
}

public class ValidationBehaviorTests
{
    // Helpers to build a trackable next delegate (NSubstitute cannot mock
    // delegate types directly).
    private static (RequestHandlerDelegate<string> Delegate, Func<int> CallCount) MakeNext(string returns = "ok")
    {
        var count = 0;
        RequestHandlerDelegate<string> del = _ => { count++; return Task.FromResult(returns); };
        return (del, () => count);
    }

    // -----------------------------------------------------------------------
    // No validators registered
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_When_NoValidatorsRegistered_Calls_Next()
    {
        var (next, callCount) = MakeNext();
        var sut = new ValidationBehavior<TestRequest, string>([]);

        await sut.Handle(new TestRequest("hello"), next, CancellationToken.None);

        callCount().ShouldBe(1);
    }

    [Fact]
    public async Task Handle_When_NoValidatorsRegistered_Returns_NextResult()
    {
        var (next, _) = MakeNext("expected");
        var sut = new ValidationBehavior<TestRequest, string>([]);

        var result = await sut.Handle(new TestRequest("hello"), next, CancellationToken.None);

        result.ShouldBe("expected");
    }

    // -----------------------------------------------------------------------
    // Validation passes
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_When_ValidatorPasses_Calls_Next()
    {
        var (next, callCount) = MakeNext();
        var sut = new ValidationBehavior<TestRequest, string>([new TestRequestAlwaysPassValidator()]);

        await sut.Handle(new TestRequest("hello"), next, CancellationToken.None);

        callCount().ShouldBe(1);
    }

    [Fact]
    public async Task Handle_When_ValidatorPasses_Returns_NextResult()
    {
        var (next, _) = MakeNext("result");
        var sut = new ValidationBehavior<TestRequest, string>([new TestRequestAlwaysPassValidator()]);

        var result = await sut.Handle(new TestRequest("hello"), next, CancellationToken.None);

        result.ShouldBe("result");
    }

    // -----------------------------------------------------------------------
    // Validation fails
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_When_ValidatorFails_Throws_ValidationException()
    {
        var (next, _) = MakeNext();
        var sut = new ValidationBehavior<TestRequest, string>([new TestRequestAlwaysFailValidator()]);

        await Should.ThrowAsync<ValidationException>(
            () => sut.Handle(new TestRequest(""), next, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_When_ValidatorFails_DoesNotCall_Next()
    {
        var (next, callCount) = MakeNext();
        var sut = new ValidationBehavior<TestRequest, string>([new TestRequestAlwaysFailValidator()]);

        try { await sut.Handle(new TestRequest(""), next, CancellationToken.None); }
        catch (ValidationException) { }

        callCount().ShouldBe(0);
    }

    [Fact]
    public async Task Handle_When_MultipleValidatorsFail_Throws_ValidationExceptionContainingAllErrors()
    {
        var (next, _) = MakeNext();
        var sut = new ValidationBehavior<TestRequest, string>([
            new TestRequestAlwaysFailValidator("Error one."),
            new TestRequestAlwaysFailValidator("Error two.")
        ]);

        var ex = await Should.ThrowAsync<ValidationException>(
            () => sut.Handle(new TestRequest(""), next, CancellationToken.None));

        ex.Errors.ShouldContain(e => e.ErrorMessage == "Error one.");
        ex.Errors.ShouldContain(e => e.ErrorMessage == "Error two.");
    }

    // -----------------------------------------------------------------------
    // Result-returning handlers: validation flows through the railway
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Handle_When_ResultResponseAndValidatorFails_Returns_FailedResultWithValidationError()
    {
        RequestHandlerDelegate<Result<string, Error>> next =
            _ => Task.FromResult(Result.Success<string, Error>("ok"));
        var sut = new ValidationBehavior<TestResultRequest, Result<string, Error>>(
            [new TestResultRequestAlwaysFailValidator()]);

        var result = await sut.Handle(new TestResultRequest(""), next, CancellationToken.None);

        result.IsFailure.ShouldBeTrue();
        var validationError = result.Error.ShouldBeOfType<ValidationError>();
        validationError.Failures.ShouldContainKey(nameof(TestResultRequest.Value));
    }

    [Fact]
    public async Task Handle_When_ResultResponseAndValidatorFails_DoesNotCall_Next()
    {
        var called = false;
        RequestHandlerDelegate<Result<string, Error>> next =
            _ => { called = true; return Task.FromResult(Result.Success<string, Error>("ok")); };
        var sut = new ValidationBehavior<TestResultRequest, Result<string, Error>>(
            [new TestResultRequestAlwaysFailValidator()]);

        await sut.Handle(new TestResultRequest(""), next, CancellationToken.None);

        called.ShouldBeFalse();
    }

    [Fact]
    public async Task Handle_When_ResultResponseAndValidatorPasses_Returns_NextResult()
    {
        RequestHandlerDelegate<Result<string, Error>> next =
            _ => Task.FromResult(Result.Success<string, Error>("passed"));
        var sut = new ValidationBehavior<TestResultRequest, Result<string, Error>>(
            [new TestResultRequestAlwaysPassValidator()]);

        var result = await sut.Handle(new TestResultRequest("hi"), next, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe("passed");
    }
}
