using Barkfest.API.Startup;

namespace Barkfest.API.Tests.Startup;

public class AdminSeedSettingsTests
{
    private static AdminSeedSettings Complete() => new()
    {
        Username = "admin",
        Name = "Admin",
        Email = "admin@example.com",
        PhoneNumber = "+15555550100",
        Password = "Admin1234!"
    };

    [Fact]
    public void IsComplete_When_AllValuesSet_Returns_True()
    {
        Complete().IsComplete.ShouldBeTrue();
    }

    [Fact]
    public void IsComplete_When_NothingConfigured_Returns_False()
    {
        new AdminSeedSettings().IsComplete.ShouldBeFalse();
    }

    [Theory]
    [InlineData(nameof(AdminSeedSettings.Username))]
    [InlineData(nameof(AdminSeedSettings.Name))]
    [InlineData(nameof(AdminSeedSettings.Email))]
    [InlineData(nameof(AdminSeedSettings.PhoneNumber))]
    [InlineData(nameof(AdminSeedSettings.Password))]
    public void IsComplete_When_ValueIsWhitespace_Returns_False(string property)
    {
        var c = Complete();
        var settings = new AdminSeedSettings
        {
            Username = property == nameof(c.Username) ? " " : c.Username,
            Name = property == nameof(c.Name) ? " " : c.Name,
            Email = property == nameof(c.Email) ? " " : c.Email,
            PhoneNumber = property == nameof(c.PhoneNumber) ? " " : c.PhoneNumber,
            Password = property == nameof(c.Password) ? " " : c.Password
        };

        settings.IsComplete.ShouldBeFalse();
    }
}
