using Barkfest.Domain.Entities;

namespace Barkfest.Tests.Common.Builders;

public class OwnerBuilder
{
    private string _username = $"user{Guid.NewGuid():N}";
    private string _firstName = "Test";
    private string _lastName = "Owner";
    private string _email = $"test.{Guid.NewGuid():N}@example.com";
    private string? _phoneNumber = null;
    private string _passwordHash = "hashed-password";
    private (string BlobName, string ContentType)? _profileImage = null;

    public OwnerBuilder WithUsername(string username)
    {
        _username = username;
        return this;
    }

    public OwnerBuilder WithFirstName(string firstName)
    {
        _firstName = firstName;
        return this;
    }

    public OwnerBuilder WithLastName(string lastName)
    {
        _lastName = lastName;
        return this;
    }

    public OwnerBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    public OwnerBuilder WithPhoneNumber(string? phoneNumber)
    {
        _phoneNumber = phoneNumber;
        return this;
    }

    public OwnerBuilder WithPasswordHash(string passwordHash)
    {
        _passwordHash = passwordHash;
        return this;
    }

    public OwnerBuilder WithProfileImage(string blobName, string contentType)
    {
        _profileImage = (blobName, contentType);
        return this;
    }

    public Owner Build()
    {
        var owner = Owner.Create(_username, _firstName, _lastName, _email, _passwordHash, _phoneNumber);
        if (_profileImage.HasValue)
            owner.SetProfileImage(_profileImage.Value.BlobName, _profileImage.Value.ContentType);
        return owner;
    }
}
