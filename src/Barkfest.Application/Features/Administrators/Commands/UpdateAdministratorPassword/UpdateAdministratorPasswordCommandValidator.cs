using Barkfest.Domain.ValueObjects;
using FluentValidation;

namespace Barkfest.Application.Features.Administrators.Commands.UpdateAdministratorPassword;

public class UpdateAdministratorPasswordCommandValidator : AbstractValidator<UpdateAdministratorPasswordCommand>
{
    public UpdateAdministratorPasswordCommandValidator()
    {
        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required.")
            .MinimumLength(AccountConstraints.PasswordMinLength)
            .WithMessage($"New password must be at least {AccountConstraints.PasswordMinLength} characters.")
            .MaximumLength(AccountConstraints.PasswordMaxLength)
            .WithMessage($"New password cannot exceed {AccountConstraints.PasswordMaxLength} characters.");
    }
}
