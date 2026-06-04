using Barkfest.Application.Features.Administrators.Commands.UpdateAdministratorPassword;
using Barkfest.Domain.ValueObjects;

namespace Barkfest.Application.Tests.Features.Administrators.Commands;

public class UpdateAdministratorPasswordCommandValidatorTests
{
    private readonly UpdateAdministratorPasswordCommandValidator _updateAdministratorPasswordCommandValidator = new();

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Fails_ForNewPassword_When_Empty(string password)
    {
        var result = _updateAdministratorPasswordCommandValidator.Validate(
            new UpdateAdministratorPasswordCommand(Guid.NewGuid(), password));

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(UpdateAdministratorPasswordCommand.NewPassword));
    }

    [Fact]
    public void Fails_ForNewPassword_When_BelowMinLength()
    {
        var shortPassword = new string('a', AccountConstraints.PasswordMinLength - 1);

        var result = _updateAdministratorPasswordCommandValidator.Validate(
            new UpdateAdministratorPasswordCommand(Guid.NewGuid(), shortPassword));

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(UpdateAdministratorPasswordCommand.NewPassword));
    }

    [Fact]
    public void Fails_ForNewPassword_When_ExceedsMaxLength()
    {
        var longPassword = new string('a', AccountConstraints.PasswordMaxLength + 1);

        var result = _updateAdministratorPasswordCommandValidator.Validate(
            new UpdateAdministratorPasswordCommand(Guid.NewGuid(), longPassword));

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(UpdateAdministratorPasswordCommand.NewPassword));
    }
}
