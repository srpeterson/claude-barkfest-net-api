using Barkfest.Application.Features.Administrators.Commands.UpdateAdministratorPassword;

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
}
