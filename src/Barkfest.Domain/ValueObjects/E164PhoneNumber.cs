using System.Text.RegularExpressions;

namespace Barkfest.Domain.ValueObjects;

public static class E164PhoneNumber
{
    public const string Pattern = @"^\+[1-9]\d{1,14}$";
    public const int MaxLength = 25;

    private static readonly Regex Regex = new(Pattern, RegexOptions.Compiled);

    public static bool IsValid(string phoneNumber) => Regex.IsMatch(phoneNumber);
}
