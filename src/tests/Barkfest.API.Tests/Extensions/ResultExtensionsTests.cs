using Barkfest.API.Extensions;
using Barkfest.Domain.Errors;
using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Barkfest.API.Tests.Extensions;

public class ResultExtensionsTests
{
    public static IEnumerable<object[]> ErrorCases =>
    [
        [new NotFoundError(nameof(Object), Guid.NewGuid()), 404],
        [new ValidationError(new Dictionary<string, string[]>()), 400],
        [new ForbiddenError(), 403],
        [new DomainRuleError("bad"), 400]
    ];

    [Theory]
    [MemberData(nameof(ErrorCases))]
    public void ToActionResult_When_Failure_Maps_ErrorToStatus(Error error, int expectedStatus)
    {
        var result = Result.Failure<string, Error>(error);

        var actionResult = result.ToActionResult();

        var objectResult = actionResult.ShouldBeOfType<ObjectResult>();
        objectResult.StatusCode.ShouldBe(expectedStatus);
        objectResult.ContentTypes.ShouldContain("application/problem+json");
    }

    [Fact]
    public void ToActionResult_When_Success_Returns_OkWithValue()
    {
        var result = Result.Success<string, Error>("hello");

        var actionResult = result.ToActionResult();

        var ok = actionResult.ShouldBeOfType<OkObjectResult>();
        ok.Value.ShouldBe("hello");
    }

    [Fact]
    public void ToNoContentResult_When_Success_Returns_NoContent()
    {
        var result = Result.Success<Unit, Error>(default);

        var actionResult = result.ToNoContentResult();

        actionResult.ShouldBeOfType<NoContentResult>();
    }

    // Guards the '_ => throw' arm in ResultExtensions.Problem: if someone adds a new
    // Error case without mapping it, this fails and points them at the mapping + ErrorCases.
    [Fact]
    public void EveryConcreteErrorType_Is_CoveredByMapping()
    {
        var concreteErrorTypes = typeof(Error).Assembly.GetTypes()
            .Where(t => t.IsSealed && t is { IsAbstract: false } && typeof(Error).IsAssignableFrom(t))
            .ToList();

        concreteErrorTypes.Count.ShouldBe(
            ErrorCases.Count(),
            "A new Error case exists. Map it in ResultExtensions.Problem and add it to ErrorCases.");
    }
}
