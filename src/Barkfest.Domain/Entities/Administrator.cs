using Barkfest.Domain.Exceptions;
using Barkfest.Domain.ValueObjects;

namespace Barkfest.Domain.Entities;

public class Administrator
{
    public const int NameMaxLength = 100;

    public Guid Id { get; private set; } = Guid.CreateVersion7();
    public string Username { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PhoneNumber { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public void SetUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new DomainException("Username is required.");

        var trimmed = username.Trim();

        if (trimmed.Length > AccountConstraints.UsernameMaxLength)
            throw new DomainException($"Username cannot exceed {AccountConstraints.UsernameMaxLength} characters.");

        Username = trimmed;
    }

    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Name is required.");

        var trimmed = name.Trim();

        if (trimmed.Length > NameMaxLength)
            throw new DomainException($"Name cannot exceed {NameMaxLength} characters.");

        Name = trimmed;
    }

    public void SetPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new DomainException("Phone number is required.");

        var trimmed = phoneNumber.Trim();

        if (trimmed.Length > E164PhoneNumber.MaxLength)
            throw new DomainException($"Phone number cannot exceed {E164PhoneNumber.MaxLength} characters.");

        if (!E164PhoneNumber.IsValid(trimmed))
            throw new DomainException("Phone number must be in E.164 format (e.g. +15555555555).");

        PhoneNumber = trimmed;
    }

    public void SetEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Email is required.");

        var trimmed = email.Trim().ToLowerInvariant();

        if (trimmed.Length > AccountConstraints.EmailMaxLength)
            throw new DomainException($"Email cannot exceed {AccountConstraints.EmailMaxLength} characters.");

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
