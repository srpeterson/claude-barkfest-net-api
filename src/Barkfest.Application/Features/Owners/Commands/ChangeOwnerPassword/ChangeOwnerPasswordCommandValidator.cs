using Barkfest.Domain.ValueObjects;
using FluentValidation;

namespace Barkfest.Application.Features.Owners.Commands.ChangeOwnerPassword;

public class ChangeOwnerPasswordCommandValidator : AbstractValidator<ChangeOwnerPasswordCommand>
{
    public ChangeOwnerPasswordCommandValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Current password is required.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required.")
            .MinimumLength(AccountConstraints.PasswordMinLength)
            .WithMessage($"Password must be at least {AccountConstraints.PasswordMinLength} characters.")
            .MaximumLength(AccountConstraints.PasswordMaxLength)
            .WithMessage($"Password must be at most {AccountConstraints.PasswordMaxLength} characters.")
            .NotEqual(x => x.CurrentPassword)
            .WithMessage("New password must be different from your current password.");
    }
}
