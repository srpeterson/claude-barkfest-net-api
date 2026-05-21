using Barkfest.Domain.Entities;
using Barkfest.Domain.ValueObjects;
using FluentValidation;

namespace Barkfest.Application.Features.Administrators.Commands.CreateAdministrator;

public class CreateAdministratorCommandValidator : AbstractValidator<CreateAdministratorCommand>
{
    public CreateAdministratorCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required.")
            .MaximumLength(AccountConstraints.UsernameMaxLength)
            .WithMessage($"Username cannot exceed {AccountConstraints.UsernameMaxLength} characters.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(Administrator.NameMaxLength)
            .WithMessage($"Name cannot exceed {Administrator.NameMaxLength} characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .MaximumLength(AccountConstraints.EmailMaxLength)
            .EmailAddress().WithMessage("Email must be a valid email address.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required.")
            .MaximumLength(E164PhoneNumber.MaxLength)
            .WithMessage($"Phone number cannot exceed {E164PhoneNumber.MaxLength} characters.")
            .Matches(E164PhoneNumber.Pattern)
            .WithMessage("Phone number must be in E.164 format (e.g. +15555555555).");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(AccountConstraints.PasswordMinLength)
            .WithMessage($"Password must be at least {AccountConstraints.PasswordMinLength} characters.")
            .MaximumLength(AccountConstraints.PasswordMaxLength)
            .WithMessage($"Password cannot exceed {AccountConstraints.PasswordMaxLength} characters.");
    }
}
