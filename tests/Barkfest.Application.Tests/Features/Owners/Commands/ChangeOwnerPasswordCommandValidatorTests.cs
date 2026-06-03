using Barkfest.Application.Features.Owners.Commands.ChangeOwnerPassword;
using Barkfest.Domain.ValueObjects;

namespace Barkfest.Application.Tests.Features.Owners.Commands;

public class ChangeOwnerPasswordCommandValidatorTests
{
    private readonly ChangeOwnerPasswordCommandValidator _changeOwnerPasswordCommandValidator = new();

    private static ChangeOwnerPasswordCommand ValidCommand(
        string currentPassword = "OldPassword1!",
        string newPassword = "NewPassword1!") =>
        new(Guid.NewGuid(), currentPassword, newPassword);

    // -----------------------------------------------------------------------
    // Valid commands
    // -----------------------------------------------------------------------

    [Fact]
    public void Validate_When_PasswordsAreValidAndDifferent_Passes()
    {
        var result = _changeOwnerPasswordCommandValidator.Validate(ValidCommand());

        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_When_NewPasswordIsAtMinLength_Passes()
    {
        var newPassword = new string('a', AccountConstraints.PasswordMinLength);

        var result = _changeOwnerPasswordCommandValidator.Validate(ValidCommand(newPassword: newPassword));

        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_When_NewPasswordIsAtMaxLength_Passes()
    {
        var newPassword = new string('a', AccountConstraints.PasswordMaxLength);

        var result = _changeOwnerPasswordCommandValidator.Validate(ValidCommand(newPassword: newPassword));

        result.IsValid.ShouldBeTrue();
    }

    // -----------------------------------------------------------------------
    // CurrentPassword
    // -----------------------------------------------------------------------

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_When_CurrentPasswordIsEmpty_Fails_ForCurrentPassword(string currentPassword)
    {
        var result = _changeOwnerPasswordCommandValidator.Validate(ValidCommand(currentPassword: currentPassword));

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(ChangeOwnerPasswordCommand.CurrentPassword));
    }

    // -----------------------------------------------------------------------
    // NewPassword
    // -----------------------------------------------------------------------

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_When_NewPasswordIsEmpty_Fails_ForNewPassword(string newPassword)
    {
        var result = _changeOwnerPasswordCommandValidator.Validate(ValidCommand(newPassword: newPassword));

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(ChangeOwnerPasswordCommand.NewPassword));
    }

    [Fact]
    public void Validate_When_NewPasswordIsBelowMinLength_Fails_ForNewPassword()
    {
        var newPassword = new string('a', AccountConstraints.PasswordMinLength - 1);

        var result = _changeOwnerPasswordCommandValidator.Validate(ValidCommand(newPassword: newPassword));

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(ChangeOwnerPasswordCommand.NewPassword));
    }

    [Fact]
    public void Validate_When_NewPasswordExceedsMaxLength_Fails_ForNewPassword()
    {
        var newPassword = new string('a', AccountConstraints.PasswordMaxLength + 1);

        var result = _changeOwnerPasswordCommandValidator.Validate(ValidCommand(newPassword: newPassword));

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(ChangeOwnerPasswordCommand.NewPassword));
    }

    [Fact]
    public void Validate_When_NewPasswordSameAsCurrentPassword_Fails_ForNewPassword()
    {
        var result = _changeOwnerPasswordCommandValidator.Validate(ValidCommand(
            currentPassword: "SharedPassword1!",
            newPassword: "SharedPassword1!"));

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(ChangeOwnerPasswordCommand.NewPassword));
    }
}
