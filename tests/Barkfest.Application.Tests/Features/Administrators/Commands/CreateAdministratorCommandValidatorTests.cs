using Barkfest.Application.Features.Administrators.Commands.CreateAdministrator;
using Barkfest.Domain.Entities;

namespace Barkfest.Application.Tests.Features.Administrators.Commands;

public class CreateAdministratorCommandValidatorTests
{
    private readonly CreateAdministratorCommandValidator _createAdministratorCommandValidator = new();

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Fails_ForEmail_When_Empty(string email)
    {
        var result = _createAdministratorCommandValidator.Validate(new CreateAdministratorCommand(email, "pass"));

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateAdministratorCommand.Email));
    }

    [Fact]
    public void Fails_ForEmail_When_ExceedsMaxLength()
    {
        var longEmail = new string('a', Administrator.EmailMaxLength) + "@b.com";

        var result = _createAdministratorCommandValidator.Validate(new CreateAdministratorCommand(longEmail, "pass"));

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateAdministratorCommand.Email));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Fails_ForPassword_When_Empty(string password)
    {
        var result = _createAdministratorCommandValidator.Validate(new CreateAdministratorCommand("new@barkfest.dev", password));

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateAdministratorCommand.Password));
    }
}
