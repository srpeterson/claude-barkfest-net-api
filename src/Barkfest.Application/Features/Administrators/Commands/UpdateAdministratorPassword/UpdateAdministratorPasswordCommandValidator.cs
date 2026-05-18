using FluentValidation;

namespace Barkfest.Application.Features.Administrators.Commands.UpdateAdministratorPassword;

public class UpdateAdministratorPasswordCommandValidator : AbstractValidator<UpdateAdministratorPasswordCommand>
{
    public UpdateAdministratorPasswordCommandValidator()
    {
        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required.");
    }
}
