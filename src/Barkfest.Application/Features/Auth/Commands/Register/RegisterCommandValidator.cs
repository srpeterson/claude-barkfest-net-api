using Barkfest.Domain.Entities;
using Barkfest.Domain.ValueObjects;
using FluentValidation;

namespace Barkfest.Application.Features.Auth.Commands.Register;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required.")
            .MaximumLength(AccountConstraints.UsernameMaxLength)
            .WithMessage($"Username cannot exceed {AccountConstraints.UsernameMaxLength} characters.");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(Owner.FirstNameMaxLength)
            .WithMessage($"First name cannot exceed {Owner.FirstNameMaxLength} characters.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(Owner.LastNameMaxLength)
            .WithMessage($"Last name cannot exceed {Owner.LastNameMaxLength} characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .MaximumLength(AccountConstraints.EmailMaxLength)
            .WithMessage($"Email cannot exceed {AccountConstraints.EmailMaxLength} characters.")
            .Matches(@"^[^@\s]+@[^@\s]+\.[^@\s]+$").WithMessage("Email must be a valid email address.");

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(E164PhoneNumber.MaxLength)
            .WithMessage($"Phone number cannot exceed {E164PhoneNumber.MaxLength} characters.")
            .Matches(E164PhoneNumber.Pattern)
            .WithMessage("Phone number must be in E.164 format (e.g. +15555555555).")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(AccountConstraints.PasswordMinLength)
            .WithMessage($"Password must be at least {AccountConstraints.PasswordMinLength} characters.")
            .MaximumLength(AccountConstraints.PasswordMaxLength)
            .WithMessage($"Password cannot exceed {AccountConstraints.PasswordMaxLength} characters.");

        RuleFor(x => x.DisplayName)
            .MaximumLength(Owner.DisplayNameMaxLength)
            .WithMessage($"Display name cannot exceed {Owner.DisplayNameMaxLength} characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.DisplayName));
    }
}
