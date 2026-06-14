namespace Barkfest.Domain.Errors;

/// <summary>
/// Closed hierarchy of modeled, expected failures. Each case is a pure data carrier;
/// the HTTP mapping lives in the API layer (ResultExtensions.ToActionResult), keeping
/// the domain ignorant of transport concerns. Genuinely exceptional/infrastructural
/// failures are NOT modeled here - they remain exceptions handled by middleware (500).
/// </summary>
public abstract record Error;

/// <summary>An aggregate could not be found. Maps to 404.</summary>
/// <param name="Entity">The entity type name (e.g. nameof(Pet)).</param>
/// <param name="Key">The identifier searched for.</param>
/// <param name="Field">Optional field name when the lookup was not by primary key
/// (e.g. "username"), preserving the original not-found message wording.</param>
public sealed record NotFoundError(string Entity, object Key, string? Field = null) : Error;

/// <summary>One or more validation rules failed. Maps to 400.</summary>
/// <param name="Failures">Property name to error messages, matching ValidationProblemDetails.</param>
public sealed record ValidationError(IReadOnlyDictionary<string, string[]> Failures) : Error;

/// <summary>The caller is not permitted to perform the action. Maps to 403.</summary>
public sealed record ForbiddenError(string? Message = null) : Error;

/// <summary>A domain invariant was violated (lifted from a thrown DomainException). Maps to 400.</summary>
public sealed record DomainRuleError(string Message) : Error;
