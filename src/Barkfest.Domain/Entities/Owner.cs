using Barkfest.Domain.Exceptions;
using Barkfest.Domain.ValueObjects;

namespace Barkfest.Domain.Entities;

public class Owner
{
    public const int FirstNameMaxLength = 50;
    public const int LastNameMaxLength = 100;
    public const int EmailMaxLength = 75;
    public const int UsernameMaxLength = 50;

    public Guid Id { get; private set; } = Guid.CreateVersion7();
    public string Username { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string? PhoneNumber { get; private set; }
    public string PasswordHash { get; private set; } = string.Empty;
    public bool Active { get; private set; } = true;
    public bool IsVisible { get; private set; } = true;
    public ProfileImage? ProfileImage { get; private set; }
    public IReadOnlyCollection<Pet> Pets => _pets.AsReadOnly();
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private readonly List<Pet> _pets = [];

    public void SetUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new DomainException("Username is required.");

        var trimmed = username.Trim();

        if (trimmed.Length > UsernameMaxLength)
            throw new DomainException($"Username cannot exceed {UsernameMaxLength} characters.");

        Username = trimmed;
    }

    public void SetFirstName(string firstName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new DomainException("First name is required.");

        if (firstName.Trim().Length > FirstNameMaxLength)
            throw new DomainException($"First name cannot exceed {FirstNameMaxLength} characters.");

        FirstName = firstName.Trim();
    }

    public void SetLastName(string lastName)
    {
        if (string.IsNullOrWhiteSpace(lastName))
            throw new DomainException("Last name is required.");

        if (lastName.Trim().Length > LastNameMaxLength)
            throw new DomainException($"Last name cannot exceed {LastNameMaxLength} characters.");

        LastName = lastName.Trim();
    }

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

    public void SetPhoneNumber(string? phoneNumber) =>
        PhoneNumber = phoneNumber?.Trim();

    public void SetPasswordHash(string hash)
    {
        if (string.IsNullOrWhiteSpace(hash))
            throw new DomainException("Password hash is required.");

        PasswordHash = hash;
    }

    public void SetActive(bool active) => Active = active;

    public void SetIsVisible(bool isVisible) => IsVisible = isVisible;

    public void SetProfileImage(string blobName, string contentType) =>
        ProfileImage = ProfileImage.Create(blobName, contentType);

    public void RemoveProfileImage() =>
        ProfileImage = null;
}
