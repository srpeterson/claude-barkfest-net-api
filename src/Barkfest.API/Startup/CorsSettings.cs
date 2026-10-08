namespace Barkfest.API.Startup;

/// <summary>
/// Bound from the "Cors" configuration section.
/// </summary>
public class CorsSettings
{
    public const string SectionName = "Cors";

    public string AllowedOrigin { get; init; } = "http://localhost:5173";
}
