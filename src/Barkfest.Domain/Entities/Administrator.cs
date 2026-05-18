using Barkfest.Domain.Exceptions;

namespace Barkfest.Domain.Entities;

public class Administrator
{
    public const int EmailMaxLength = 75;

    public Guid Id { get; private set; } = Guid.CreateVersion7();
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public void SetEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Email is required.");

        var trimmed = email.Trim().ToLowerInvariant();

        if (trimmed.Length > EmailMaxLength)
            throw new DomainException($"Email cannot exceed {EmailMaxLength} characters.");

        if (trimmed.Contains(' '))
            throw new DomainException("Email must be a valid email address.");

        var atIndex = trimmed.IndexOf('@');
        if (atIndex <= 0)
            throw new DomainException("Email must be a valid email address.");

        var domain = trimmed[(atIndex + 1)..];
        if (string.IsNullOrEmpty(domain))
            throw new DomainException("Email must be a valid email address.");

        var dotIndex = domain.LastIndexOf('.');
        if (dotIndex <= 0 || dotIndex == domain.Length - 1)
            throw new DomainException("Email must be a valid email address.");

        Email = trimmed;
    }

    public void SetPasswordHash(string hash)
    {
        if (string.IsNullOrWhiteSpace(hash))
            throw new DomainException("Password hash is required.");

        PasswordHash = hash;
    }
}
