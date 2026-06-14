using System.Collections.Concurrent;
using System.Reflection;
using Barkfest.Domain.Errors;
using CSharpFunctionalExtensions;

namespace Barkfest.Application.Common.Behaviors;

/// <summary>
/// Builds a failed <see cref="Result{T, E}"/> for an arbitrary <c>TResponse</c> that is a
/// <c>Result&lt;TValue, Error&gt;</c>. MediatR pipeline behaviours are generic over the
/// concrete response type, so constructing the failure requires a small amount of cached
/// reflection. Only exercised on the validation-failure path (cold).
/// </summary>
internal static class ResultFailureFactory
{
    private static readonly ConcurrentDictionary<Type, Func<Error, object>> Cache = new();

    /// <summary>True when <c>TResponse</c> is a closed <c>Result&lt;TValue, Error&gt;</c>.</summary>
    public static bool IsResult<TResponse>()
    {
        var type = typeof(TResponse);
        return type.IsGenericType
            && type.GetGenericTypeDefinition() == typeof(Result<,>)
            && type.GetGenericArguments()[1] == typeof(Error);
    }

    /// <summary>
    /// Returns <c>Result&lt;TValue, Error&gt;.Failure(error)</c> boxed as <c>TResponse</c>.
    /// Call only when <see cref="IsResult{TResponse}"/> is true.
    /// </summary>
    public static TResponse Create<TResponse>(Error error)
    {
        var factory = Cache.GetOrAdd(typeof(TResponse), BuildFactory);
        return (TResponse)factory(error);
    }

    private static Func<Error, object> BuildFactory(Type responseType)
    {
        var valueType = responseType.GetGenericArguments()[0];

        // Result.Failure<TValue, Error>(Error error) -> Result<TValue, Error>
        var method = typeof(Result)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(Result.Failure)
                && m.IsGenericMethod
                && m.GetGenericArguments().Length == 2
                && m.GetParameters().Length == 1)
            .MakeGenericMethod(valueType, typeof(Error));

        return error => method.Invoke(null, [error])!;
    }
}
