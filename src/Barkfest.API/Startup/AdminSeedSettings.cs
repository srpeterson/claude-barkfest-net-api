namespace Barkfest.API.Startup;

/// <summary>
/// Bound from the "Admin" configuration section. The seed is skipped unless every value is set.
/// </summary>
public class AdminSeedSettings
{
    public const string SectionName = "Admin";

    public string? Username { get; init; }
    public string? Name { get; init; }
    public string? Email { get; init; }
    public string? PhoneNumber { get; init; }
    public string? Password { get; init; }

    public bool IsComplete =>
        !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Name) &&
        !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(PhoneNumber) &&
        !string.IsNullOrWhiteSpace(Password);
}
