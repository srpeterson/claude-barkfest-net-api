namespace Barkfest.Domain.ValueObjects;

public static class AccountConstraints
{
    public const int EmailMaxLength = 75;
    public const int UsernameMaxLength = 50;
    public const int PasswordMinLength = 8;
    public const int PasswordMaxLength = 72;  // BCrypt silently ignores characters beyond 72
}
